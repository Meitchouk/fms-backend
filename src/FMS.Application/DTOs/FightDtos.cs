using FMS.Domain.Enums;

namespace FMS.Application.DTOs;

// ── Fight DTOs ──────────────────────────────────────────────────────────────

public sealed record CreateFightRequest(
    Guid ParticipantAId,
    Guid ParticipantBId,
    int OrderNumber);

public sealed record UpdateFightRequest(
    Guid ParticipantAId,
    Guid ParticipantBId);

public sealed record ReorderFightsRequest(
    IReadOnlyList<ReorderItem> Fights);

public sealed record ReorderItem(
    Guid FightId,
    int NewOrderNumber);

public sealed record FightResponse(
    Guid Id,
    Guid EventId,
    Guid ParticipantAId,
    string ParticipantAName,
    Guid ParticipantBId,
    string ParticipantBName,
    int OrderNumber,
    FightStatus Status,
    FightResultResponse? Result,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record FightResultResponse(
    Guid Id,
    FightOutcome Outcome,
    string? Method,
    string? Notes);
