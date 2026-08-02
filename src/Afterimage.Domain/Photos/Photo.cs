namespace Afterimage.Domain.Photos;

/// <summary>
/// A photo record. Persisted immediately on upload in <see cref="PhotoStatus.Uploaded"/>,
/// then advanced by the Worker. State changes go through domain methods only;
/// setters are private (see ADR-0003).
/// </summary>
public sealed class Photo
{
    // EF Core materialization constructor.
    private Photo() { }

    private Photo(Guid ownerId, string fileName, string contentType, long sizeBytes)
    {
        // App-side, time-ordered id (.NET 9). Needed up front for the MinIO key
        // and the RabbitMQ message (ADR-0002).
        Id = Guid.CreateVersion7();
        OwnerId = ownerId;
        FileName = fileName;
        ContentType = contentType;
        SizeBytes = sizeBytes;
        OriginalStorageKey = $"{ownerId}/{Id}/original";
        LifecycleState = PhotoStatus.Uploaded;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; private set; }
    public Guid OwnerId { get; private set; }
    public string FileName { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;
    public long SizeBytes { get; private set; }
    public string OriginalStorageKey { get; private set; } = null!;
    public string? ThumbnailStorageKey { get; private set; }
    public PhotoStatus LifecycleState { get; private set; }
    public string? FailureReason { get; private set; }
    public PhotoMetadata? Metadata { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>Creates a freshly uploaded photo in the <c>Uploaded</c> state.</summary>
    public static Photo Create(Guid ownerId, string fileName, string contentType, long sizeBytes)
        => new(ownerId, fileName, contentType, sizeBytes);

    /// <summary>Worker finished successfully: attach thumbnail + metadata, move to <c>Processed</c>.</summary>
    public void MarkProcessed(string thumbnailStorageKey, PhotoMetadata metadata)
    {
        ThumbnailStorageKey = thumbnailStorageKey;
        Metadata = metadata;
        FailureReason = null;
        LifecycleState = PhotoStatus.Processed;
        Touch();
    }

    /// <summary>Processing failed on a bad file: record the reason, move to <c>Failed</c>.</summary>
    public void MarkFailed(string reason)
    {
        FailureReason = reason;
        LifecycleState = PhotoStatus.Failed;
        Touch();
    }

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
