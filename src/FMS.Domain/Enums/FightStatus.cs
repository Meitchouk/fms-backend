namespace FMS.Domain.Enums;

/// <summary>
/// Lifecycle status of a fight. Transitions are enforced by the saga in D03.
/// </summary>
public enum FightStatus
{
    /// <summary>Fight is created and awaiting its turn.</summary>
    Scheduled = 0,

    /// <summary>Fight has been announced to participants and audience.</summary>
    Announced = 1,

    /// <summary>Fight is actively in progress (timer running).</summary>
    InProgress = 2,

    /// <summary>Fight is temporarily paused.</summary>
    Paused = 3,

    /// <summary>Fight has concluded normally.</summary>
    Finished = 4,

    /// <summary>Fight was cancelled before or during execution.</summary>
    Cancelled = 5
}
