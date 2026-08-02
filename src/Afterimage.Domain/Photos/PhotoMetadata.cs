namespace Afterimage.Domain.Photos;

/// <summary>
/// Image/capture metadata discovered by the Worker during processing.
/// Stored as a jsonb column (see ADR-0002); all fields optional because they
/// may be missing or absent at insert time.
/// </summary>
public sealed class PhotoMetadata
{
    public int? Width { get; init; }
    public int? Height { get; init; }
    public DateTimeOffset? TakenAt { get; init; }
}
