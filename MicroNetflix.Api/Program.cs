using Microsoft.AspNetCore.Mvc;
using MassTransit;
using MicroNetflix.Shared;
using Minio;
using Minio.DataModel.Args;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAntiforgery();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

// Database
builder.Services.AddDbContext<VideoDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Host=localhost;Database=micronetflix;Username=admin;Password=password"));

// Message Bus
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"] ?? "rabbitmq", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
    });
});

// Storage
builder.Services.AddMinio(configureClient => configureClient
    .WithEndpoint(builder.Configuration["Minio:Endpoint"] ?? "minio", 9000)
    .WithCredentials("minioadmin", "minioadmin")
    .WithSSL(false)
    .Build());

var app = builder.Build();

// Ensure DB Created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VideoDbContext>();
    db.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAntiforgery();

app.MapPost("/videos/upload", async (IFormFile file, IMinioClient minio, VideoDbContext db, IPublishEndpoint publishEndpoint) =>
{
    if (file.Length == 0) return Results.BadRequest("Empty file");

    var videoId = Guid.NewGuid();
    var fileName = $"{videoId}{Path.GetExtension(file.FileName)}";

    // 1. Upload to MinIO
    using var stream = file.OpenReadStream();
    var putObjectArgs = new PutObjectArgs()
        .WithBucket("videos")
        .WithObject(fileName)
        .WithStreamData(stream)
        .WithObjectSize(stream.Length)
        .WithContentType(file.ContentType);
    
    // Ensure bucket exists
    var beArgs = new BucketExistsArgs().WithBucket("videos");
    bool found = await minio.BucketExistsAsync(beArgs);
    if (!found)
    {
        var mbArgs = new MakeBucketArgs().WithBucket("videos");
        await minio.MakeBucketAsync(mbArgs);
    }

    await minio.PutObjectAsync(putObjectArgs);

    // 2. Save Metadata
    var video = new VideoMetadata
    {
        Id = videoId,
        OriginalFileName = file.FileName,
        StoredFileName = fileName,
        UploadedAt = DateTime.UtcNow,
        Status = "Pending"
    };

    db.Videos.Add(video);
    await db.SaveChangesAsync();

    // 3. Publish Event
    await publishEndpoint.Publish(new VideoUploadedEvent(videoId, fileName));

    return Results.Ok(new { Id = videoId, Status = "Uploaded", Message = "Processing started" });
})
.WithName("UploadVideo")
.WithOpenApi()
.DisableAntiforgery();

app.MapGet("/videos/{id}", async (Guid id, VideoDbContext db) =>
{
    var video = await db.Videos.FindAsync(id);
    if (video == null) return Results.NotFound();
    
    return Results.Ok(new 
    { 
        video.Id, 
        video.Status, 
        video.OriginalFileName, 
        video.UploadedAt 
    });
})
.WithName("GetVideoStatus")
.WithOpenApi();

app.MapGet("/videos/{id}/stream", async (Guid id, VideoDbContext db, IMinioClient minio) =>
{
    var video = await db.Videos.FindAsync(id);
    if (video == null) return Results.NotFound();
    if (video.Status != "Completed") return Results.BadRequest("Video not ready");

    // Get the stream from MinIO
    // Note: In a real app, we might generate a presigned URL. 
    // For this demo, we'll proxy the stream.
    
    var memoryStream = new MemoryStream();
    var getObjectArgs = new GetObjectArgs()
        .WithBucket("videos")
        .WithObject(video.StoredFileName)
        .WithCallbackStream(stream => stream.CopyTo(memoryStream));
    
    await minio.GetObjectAsync(getObjectArgs);
    memoryStream.Position = 0;

    return Results.File(memoryStream, "video/mp4", enableRangeProcessing: true);
})
.WithName("StreamVideo")
.WithOpenApi();

app.Run();
