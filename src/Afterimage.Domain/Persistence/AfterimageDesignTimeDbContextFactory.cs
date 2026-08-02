using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Afterimage.Domain.Persistence;

/// <summary>
/// Used only by <c>dotnet ef</c> at design time (migrations), when no host is
/// running to build configuration. Reads the connection string from the
/// <c>ConnectionStrings__Postgres</c> environment variable and falls back to the
/// local compose defaults (dev-only, matching deploy/compose/.env.example — not
/// a secret). Never used at runtime.
/// </summary>
internal sealed class AfterimageDesignTimeDbContextFactory
    : IDesignTimeDbContextFactory<AfterimageDbContext>
{
    private const string LocalComposeFallback =
        "Host=localhost;Port=5432;Database=afterimage;Username=afterimage;Password=afterimage_dev_pw";

    public AfterimageDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
            ?? LocalComposeFallback;

        var options = new DbContextOptionsBuilder<AfterimageDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new AfterimageDbContext(options);
    }
}
