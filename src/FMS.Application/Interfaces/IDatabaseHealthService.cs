namespace FMS.Application.Interfaces;

/// <summary>
/// Abstraction for database connectivity validation.
/// </summary>
public interface IDatabaseHealthService
{
    /// <summary>
    /// Checks if the database is reachable and migrations are applied.
    /// </summary>
    Task<DatabaseHealthResult> CheckHealthAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a database health check.
/// </summary>
public sealed record DatabaseHealthResult(
    bool IsConnected,
    bool MigrationsApplied,
    string? PendingMigrations,
    string? Error);
