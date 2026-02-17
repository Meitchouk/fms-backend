using FMS.Domain.Common;
using FMS.Domain.Enums;

namespace FMS.Domain.Entities;

/// <summary>
/// A scheduled fight between two participants within an event.
/// </summary>
public class Fight : BaseEntity
{
    /// <summary>Foreign key to the parent event.</summary>
    public Guid EventId { get; set; }

    /// <summary>Foreign key to participant A (corner A).</summary>
    public Guid ParticipantAId { get; set; }

    /// <summary>Foreign key to participant B (corner B).</summary>
    public Guid ParticipantBId { get; set; }

    /// <summary>Position in the fight card (1-based).</summary>
    public int OrderNumber { get; set; }

    /// <summary>Current lifecycle status (managed by saga in D03).</summary>
    public FightStatus Status { get; set; } = FightStatus.Scheduled;

    // Navigation properties
    public Event Event { get; set; } = null!;
    public Participant ParticipantA { get; set; } = null!;
    public Participant ParticipantB { get; set; } = null!;
    public FightResult? Result { get; set; }
}
