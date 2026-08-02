using Afterimage.Domain.Photos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Afterimage.Domain.Persistence.Configurations;

internal sealed class PhotoConfiguration : IEntityTypeConfiguration<Photo>
{
    public void Configure(EntityTypeBuilder<Photo> builder)
    {
        builder.HasKey(p => p.Id);

        // Legible in the DB, survives enum reordering (ADR-0002).
        builder.Property(p => p.LifecycleState)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.FileName).HasMaxLength(260).IsRequired();
        builder.Property(p => p.ContentType).HasMaxLength(100).IsRequired();
        builder.Property(p => p.OriginalStorageKey).HasMaxLength(512).IsRequired();
        builder.Property(p => p.ThumbnailStorageKey).HasMaxLength(512);
        builder.Property(p => p.FailureReason).HasMaxLength(1024);

        // Discovered EXIF/capture metadata → jsonb.
        builder.OwnsOne(p => p.Metadata, b => b.ToJson());

        // Access pattern: a user's photos, filtered by state, newest first (ADR-0002).
        builder.HasIndex(p => new { p.OwnerId, p.LifecycleState, p.CreatedAt })
            .IsDescending(false, false, true);
    }
}
