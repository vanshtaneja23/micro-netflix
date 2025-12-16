namespace MicroNetflix.Shared;

public class VideoMetadata
{
    public Guid Id { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Processing, Completed, Failed
}
