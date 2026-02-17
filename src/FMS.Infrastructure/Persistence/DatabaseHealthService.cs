using FMS.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace FMS.Infrastructure.Persistence;

/// <summary>
/// Implementation of IDatabaseHealthService using EF Core.
/// Validates connectivity and migration state.
/// </summary>
public sealed class DatabaseHealthService : IDatabaseHealthService
{
    private readonly FmsDbContext _dbContext;

    public DatabaseHealthService(FmsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DatabaseHealthResult> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Test raw connectivity
            bool canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
            if (!canConnect)
            {
                return new DatabaseHealthResult(false, false, null, "Cannot connect to database");
            }

            // Check pending migrations
            var pendingMigrations = await _dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
            var pendingList = pendingMigrations.ToList();

            return new DatabaseHealthResult(
                IsConnected: true,
                MigrationsApplied: pendingList.Count == 0,
                PendingMigrations: pendingList.Count > 0 ? string.Join(", ", pendingList) : null,
                Error: null);
        }
        catch (Exception ex)
        {
            return new DatabaseHealthResult(false, false, null, ex.Message);
        }
    }
}
