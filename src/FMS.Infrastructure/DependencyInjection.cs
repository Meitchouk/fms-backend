using FMS.Application.Interfaces;
using FMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FMS.Infrastructure;

/// <summary>
/// Extension methods to register Infrastructure services in the DI container.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // PostgreSQL + EF Core
        var connectionString = configuration.GetConnectionString("FmsDatabase")
            ?? throw new InvalidOperationException("Connection string 'FmsDatabase' is not configured.");

        services.AddDbContext<FmsDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(FmsDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
            }));

        // Application service registrations
        services.AddScoped<IDatabaseHealthService, DatabaseHealthService>();

        return services;
    }
}
