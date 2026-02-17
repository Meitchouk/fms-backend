namespace FMS.Application.DTOs;

// ── Team DTOs ───────────────────────────────────────────────────────────────

public sealed record CreateTeamRequest(string Name);

public sealed record UpdateTeamRequest(string Name);

public sealed record TeamResponse(
    Guid Id,
    string Name,
    Guid EventId,
    int ParticipantCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
