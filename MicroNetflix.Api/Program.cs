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

app.MapPost("/videos/upload", async (IFormFile file) =>
{
    // TODO: Implement upload logic
    return Results.Ok(new { Message = "Video uploaded successfully" });
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
