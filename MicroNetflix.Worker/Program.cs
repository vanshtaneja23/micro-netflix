using MassTransit;
using MicroNetflix.Worker;
using Minio;
using Microsoft.EntityFrameworkCore;
using MicroNetflix.Shared;

var builder = Host.CreateApplicationBuilder(args);

// Storage
builder.Services.AddMinio(configureClient => configureClient
    .WithEndpoint(builder.Configuration["Minio:Endpoint"] ?? "minio", 9000)
    .WithCredentials("minioadmin", "minioadmin")
    .WithSSL(false)
    .Build());

// Database
builder.Services.AddDbContext<VideoDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Message Bus
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<VideoUploadedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"] ?? "rabbitmq", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("video-processing-queue", e =>
        {
            e.ConfigureConsumer<VideoUploadedConsumer>(context);
        });
    });
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
