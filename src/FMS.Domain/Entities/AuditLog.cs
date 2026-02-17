using FMS.Domain.Common;

namespace FMS.Domain.Entities;

/// <summary>
/// Baseline audit log entry for tracking changes to domain entities.
/// </summary>
public class AuditLog : BaseEntity
{
    /// <summary>Type name of the entity that was changed.</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>Id of the entity that was changed.</summary>
    public Guid EntityId { get; set; }

    /// <summary>Description of the action performed (e.g., "Created", "Updated", "StatusChanged").</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>Optional JSON details about the change.</summary>
    public string? Details { get; set; }

    /// <summary>Timestamp of the action.</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
