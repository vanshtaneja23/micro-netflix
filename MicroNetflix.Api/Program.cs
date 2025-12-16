using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<VideoDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Host=localhost;Database=micronetflix;Username=admin;Password=password"));

// Message Bus
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
    });
});

// Storage
builder.Services.AddMinio(configureClient => configureClient
    .WithEndpoint("localhost", 9000)
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

using MassTransit;
using MicroNetflix.Api;
using MicroNetflix.Shared;
using Minio;
using Minio.DataModel.Args;

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
.WithOpenApi();

app.MapGet("/videos/{id}", (Guid id) =>
{
    // TODO: Implement status check
    return Results.Ok(new { Id = id, Status = "Processing" });
})
.WithName("GetVideoStatus")
.WithOpenApi();

app.Run();
