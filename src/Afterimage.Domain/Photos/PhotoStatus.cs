namespace Afterimage.Domain.Photos;

/// <summary>Processing lifecycle state of a photo. Persisted as text (see ADR-0002).</summary>
public enum PhotoStatus
{
    Uploaded,
    Processed,
    Failed,
}
