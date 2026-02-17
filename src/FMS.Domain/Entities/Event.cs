using FMS.Domain.Common;
using FMS.Domain.Enums;

namespace FMS.Domain.Entities;

/// <summary>
/// Scoring configuration for an event discipline.
/// Owned entity — persisted inside the Event table.
/// </summary>
public class ScoringConfig
{
    /// <summary>Points awarded for a victory.</summary>
    public int VictoryPoints { get; set; } = 3;

    /// <summary>Points awarded for a draw.</summary>
    public int DrawPoints { get; set; } = 1;

    /// <summary>Points awarded for a defeat.</summary>
    public int DefeatPoints { get; set; } = 0;

    /// <summary>Whether KO victories are tracked.</summary>
    public bool AllowKo { get; set; }

    /// <summary>Whether TKO victories are tracked.</summary>
    public bool AllowTko { get; set; }
}

/// <summary>
/// Discipline and rules configuration for an event.
/// Owned entity — persisted inside the Event table.
/// </summary>
public class DisciplineConfig
{
    /// <summary>Name of the discipline (e.g., "Kickboxing", "MMA").</summary>
    public string DisciplineName { get; set; } = string.Empty;

    /// <summary>Timer behavior: continuous or rounds-based.</summary>
    public TimerMode TimerMode { get; set; } = TimerMode.Continuous;

    /// <summary>Number of rounds (only used when TimerMode is RoundsBased).</summary>
    public int? RoundCount { get; set; }

    /// <summary>Duration of each round in seconds.</summary>
    public int? RoundDurationSeconds { get; set; }

    /// <summary>Rest duration between rounds in seconds.</summary>
    public int? RestDurationSeconds { get; set; }

    /// <summary>Scoring rules for this discipline.</summary>
    public ScoringConfig Scoring { get; set; } = new();
}

/// <summary>
/// Represents a fight event containing fights, participants, and teams.
/// </summary>
public class Event : BaseEntity
{
    /// <summary>Display name of the event.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Current lifecycle status of the event.</summary>
    public EventStatus Status { get; set; } = EventStatus.Draft;

    /// <summary>Discipline and rules configuration.</summary>
    public DisciplineConfig Discipline { get; set; } = new();

    // Navigation properties
    public ICollection<Team> Teams { get; set; } = new List<Team>();
    public ICollection<Participant> Participants { get; set; } = new List<Participant>();
    public ICollection<Fight> Fights { get; set; } = new List<Fight>();
}
