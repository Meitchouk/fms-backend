using Microsoft.EntityFrameworkCore;

namespace FMS.Infrastructure.Persistence;

/// <summary>
/// Main EF Core DbContext for the FMS system.
/// All domain entities will be registered here as the system evolves.
/// </summary>
public class FmsDbContext : DbContext
{
    public FmsDbContext(DbContextOptions<FmsDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FmsDbContext).Assembly);
    }

    /// <summary>
    /// Override SaveChanges to automatically set UpdatedAt timestamps.
    /// </summary>
    public override int SaveChanges()
    {
        SetTimestamps();
        return base.SaveChanges();
    }

    /// <summary>
    /// Override SaveChangesAsync to automatically set UpdatedAt timestamps.
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void SetTimestamps()
    {
        var entries = ChangeTracker.Entries<Domain.Common.BaseEntity>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
