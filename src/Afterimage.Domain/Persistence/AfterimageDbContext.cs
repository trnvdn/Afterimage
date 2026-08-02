using Afterimage.Domain.Photos;
using Microsoft.EntityFrameworkCore;

namespace Afterimage.Domain.Persistence;

public sealed class AfterimageDbContext : DbContext
{
    public AfterimageDbContext(DbContextOptions<AfterimageDbContext> options)
        : base(options)
    {
    }

    public DbSet<Photo> Photos => Set<Photo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Picks up internal IEntityTypeConfiguration<T> in this assembly (ADR-0003).
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AfterimageDbContext).Assembly);
    }
}
