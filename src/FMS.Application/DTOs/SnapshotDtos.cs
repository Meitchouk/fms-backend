using FMS.Domain.Enums;

namespace FMS.Application.DTOs;

// ── Snapshot DTOs (read-only for TV/Judge hydration) ────────────────────────

public sealed record EventSnapshotResponse(
    Guid Id,
    string Name,
    EventStatus Status,
    DisciplineConfigDto Discipline,
    IReadOnlyList<TeamResponse> Teams,
    IReadOnlyList<ParticipantResponse> Participants,
    IReadOnlyList<FightResponse> Fights);
