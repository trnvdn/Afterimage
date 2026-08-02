using Afterimage.Domain.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Afterimage.Domain;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the persistence layer. The connection string is supplied by the
    /// host (Api / Worker) from its own configuration — Domain never sources it
    /// itself, so it has no knowledge of where the app runs (ADR-0003).
    /// </summary>
    public static IServiceCollection AddAfterimagePersistence(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<AfterimageDbContext>(options =>
            options
                .UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention());

        return services;
    }
}
