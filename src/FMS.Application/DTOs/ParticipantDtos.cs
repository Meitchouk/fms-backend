namespace FMS.Application.DTOs;

// ── Participant DTOs ────────────────────────────────────────────────────────

public sealed record CreateParticipantRequest(
    string Name,
    decimal? Weight,
    Guid? TeamId);

public sealed record UpdateParticipantRequest(
    string Name,
    decimal? Weight,
    Guid? TeamId);

public sealed record ParticipantResponse(
    Guid Id,
    string Name,
    decimal? Weight,
    Guid EventId,
    Guid? TeamId,
    string? TeamName,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
