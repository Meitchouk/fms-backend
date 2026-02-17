namespace FMS.Domain.Enums;

/// <summary>
/// Outcome of a completed fight.
/// </summary>
public enum FightOutcome
{
    /// <summary>Participant A wins.</summary>
    WinnerA = 0,

    /// <summary>Participant B wins.</summary>
    WinnerB = 1,

    /// <summary>The fight ended in a draw.</summary>
    Draw = 2
}
