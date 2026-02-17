namespace FMS.Domain.Enums;

/// <summary>
/// Timer behavior for fights within an event discipline.
/// </summary>
public enum TimerMode
{
    /// <summary>Single continuous countdown.</summary>
    Continuous = 0,

    /// <summary>Multiple rounds with rest periods between them.</summary>
    RoundsBased = 1
}
