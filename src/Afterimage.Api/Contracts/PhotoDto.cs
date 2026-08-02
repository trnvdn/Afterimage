namespace Afterimage.Api.Contracts;

/// <summary>Read model returned to clients (ADR-0004).</summary>
public sealed record PhotoDto(
    Guid Id,
    Guid OwnerId,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
