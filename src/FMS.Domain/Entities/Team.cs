using FMS.Domain.Common;

namespace FMS.Domain.Entities;

/// <summary>
/// A team that participants can belong to within an event.
/// </summary>
public class Team : BaseEntity
{
    /// <summary>Display name of the team.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Foreign key to the parent event.</summary>
    public Guid EventId { get; set; }

    // Navigation properties
    public Event Event { get; set; } = null!;
    public ICollection<Participant> Participants { get; set; } = new List<Participant>();
}
