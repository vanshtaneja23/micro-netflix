namespace MicroNetflix.Shared;

public record VideoUploadedEvent(Guid VideoId, string FileName);
