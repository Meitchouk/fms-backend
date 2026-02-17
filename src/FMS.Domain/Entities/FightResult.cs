using FMS.Domain.Common;
using FMS.Domain.Enums;

namespace FMS.Domain.Entities;

/// <summary>
/// Result of a completed fight. Placeholder — will be extended with multi-judge support later.
/// </summary>
public class FightResult : BaseEntity
{
    /// <summary>Foreign key to the fight.</summary>
    public Guid FightId { get; set; }

    /// <summary>Outcome of the fight.</summary>
    public FightOutcome Outcome { get; set; }

    /// <summary>Method of victory (e.g., "Decision", "KO", "TKO"). Nullable for draws.</summary>
    public string? Method { get; set; }

    /// <summary>Optional notes about the result.</summary>
    public string? Notes { get; set; }

    // Navigation
    public Fight Fight { get; set; } = null!;
}
