using FMS.Domain.Common;

namespace FMS.Domain.Entities;

/// <summary>
/// A participant (fighter) registered in an event.
/// </summary>
public class Participant : BaseEntity
{
    /// <summary>Full name of the participant.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Weight of the participant (optional, used for draw generation).</summary>
    public decimal? Weight { get; set; }

    /// <summary>Foreign key to the parent event.</summary>
    public Guid EventId { get; set; }

    /// <summary>Optional foreign key to the team.</summary>
    public Guid? TeamId { get; set; }

    // Navigation properties
    public Event Event { get; set; } = null!;
    public Team? Team { get; set; }
}
