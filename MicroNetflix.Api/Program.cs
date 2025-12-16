using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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
