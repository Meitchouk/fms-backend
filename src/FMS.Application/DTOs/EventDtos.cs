using FMS.Domain.Enums;

namespace FMS.Application.DTOs;

// ── Event DTOs ──────────────────────────────────────────────────────────────

public sealed record CreateEventRequest(
    string Name,
    DisciplineConfigDto Discipline);

public sealed record UpdateEventRequest(
    string Name,
    DisciplineConfigDto Discipline);

public sealed record DisciplineConfigDto(
    string DisciplineName,
    TimerMode TimerMode,
    int? RoundCount,
    int? RoundDurationSeconds,
    int? RestDurationSeconds,
    ScoringConfigDto Scoring);

public sealed record ScoringConfigDto(
    int VictoryPoints,
    int DrawPoints,
    int DefeatPoints,
    bool AllowKo,
    bool AllowTko);

public sealed record EventResponse(
    Guid Id,
    string Name,
    EventStatus Status,
    DisciplineConfigDto Discipline,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record EventDetailResponse(
    Guid Id,
    string Name,
    EventStatus Status,
    DisciplineConfigDto Discipline,
    int TeamCount,
    int ParticipantCount,
    int FightCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
