using MassTransit;
using MicroNetflix.Shared;
using Minio;
using Minio.DataModel.Args;

namespace MicroNetflix.Worker;

public class VideoUploadedConsumer : IConsumer<VideoUploadedEvent>
{
    private readonly ILogger<VideoUploadedConsumer> _logger;
    private readonly IMinioClient _minio;

    public VideoUploadedConsumer(ILogger<VideoUploadedConsumer> logger, IMinioClient minio)
    {
        _logger = logger;
        _minio = minio;
    }

    public async Task Consume(ConsumeContext<VideoUploadedEvent> context)
    {
        var videoId = context.Message.VideoId;
        var fileName = context.Message.FileName;

        _logger.LogInformation("Received processing job for Video: {VideoId}", videoId);

        try
        {
            // 1. Simulate Download
            _logger.LogInformation("Downloading {FileName}...", fileName);
            var memoryStream = new MemoryStream();
            var getObjectArgs = new GetObjectArgs()
                .WithBucket("videos")
                .WithObject(fileName)
                .WithCallbackStream(stream => stream.CopyTo(memoryStream));
            
            await _minio.GetObjectAsync(getObjectArgs);
            memoryStream.Position = 0;

            // 2. Simulate Processing (Transcoding)
            _logger.LogInformation("Transcoding video...");
            await Task.Delay(5000); // Simulate 5 seconds of work

            // 3. Upload "Processed" version (just re-uploading for demo)
            var processedFileName = $"processed_{fileName}";
            var putObjectArgs = new PutObjectArgs()
                .WithBucket("videos")
                .WithObject(processedFileName)
                .WithStreamData(memoryStream)
                .WithObjectSize(memoryStream.Length)
                .WithContentType("video/mp4");

            await _minio.PutObjectAsync(putObjectArgs);

            _logger.LogInformation("Processing completed. Saved as {ProcessedFileName}", processedFileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing video {VideoId}", videoId);
            throw; // Retry
        }
    }
}
