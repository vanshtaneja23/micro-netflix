using MassTransit;
using MicroNetflix.Worker;
using Minio;

var builder = Host.CreateApplicationBuilder(args);

// Storage
builder.Services.AddMinio(configureClient => configureClient
    .WithEndpoint("localhost", 9000)
    .WithCredentials("minioadmin", "minioadmin")
    .WithSSL(false)
    .Build());

// Message Bus
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<VideoUploadedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
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
