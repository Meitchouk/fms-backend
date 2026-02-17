namespace FMS.Domain.Enums;

/// <summary>
/// Lifecycle status of an event.
/// </summary>
public enum EventStatus
{
    /// <summary>Event is being configured, not yet active.</summary>
    Draft = 0,

    /// <summary>Event is active and fights can be managed.</summary>
    Active = 1,

    /// <summary>Event has finished.</summary>
    Completed = 2
}
