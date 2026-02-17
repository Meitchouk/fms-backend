using FMS.Application.DTOs;
using FMS.Domain.Entities;
using FMS.Domain.Enums;
using FMS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace FMS.Api.Endpoints;

public static class EventEndpoints
{
    public static RouteGroupBuilder MapEventEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/events").WithTags("events");

        group.MapGet("/", GetAll);
        group.MapGet("/{id:guid}", GetById);
        group.MapPost("/", Create);
        group.MapPut("/{id:guid}", Update);
        group.MapPatch("/{id:guid}/status", UpdateStatus);
        group.MapGet("/{id:guid}/snapshot", GetSnapshot);

        return group;
    }

    private static async Task<Ok<List<EventResponse>>> GetAll(FmsDbContext db, CancellationToken ct)
    {
        var events = await db.Events
            .AsNoTracking()
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => MapToResponse(e))
            .ToListAsync(ct);

        return TypedResults.Ok(events);
    }

    private static async Task<Results<Ok<EventDetailResponse>, NotFound>> GetById(
        Guid id, FmsDbContext db, CancellationToken ct)
    {
        var ev = await db.Events
            .AsNoTracking()
            .Where(e => e.Id == id)
            .Select(e => new EventDetailResponse(
                e.Id,
                e.Name,
                e.Status,
                new DisciplineConfigDto(
                    e.Discipline.DisciplineName,
                    e.Discipline.TimerMode,
                    e.Discipline.RoundCount,
                    e.Discipline.RoundDurationSeconds,
                    e.Discipline.RestDurationSeconds,
                    new ScoringConfigDto(
                        e.Discipline.Scoring.VictoryPoints,
                        e.Discipline.Scoring.DrawPoints,
                        e.Discipline.Scoring.DefeatPoints,
                        e.Discipline.Scoring.AllowKo,
                        e.Discipline.Scoring.AllowTko)),
                e.Teams.Count,
                e.Participants.Count,
                e.Fights.Count,
                e.CreatedAt,
                e.UpdatedAt))
            .FirstOrDefaultAsync(ct);

        return ev is not null
            ? TypedResults.Ok(ev)
            : TypedResults.NotFound();
    }

    private static async Task<Created<EventResponse>> Create(
        CreateEventRequest request, FmsDbContext db, CancellationToken ct)
    {
        var entity = new Event
        {
            Name = request.Name,
            Status = EventStatus.Draft,
            Discipline = new DisciplineConfig
            {
                DisciplineName = request.Discipline.DisciplineName,
                TimerMode = request.Discipline.TimerMode,
                RoundCount = request.Discipline.RoundCount,
                RoundDurationSeconds = request.Discipline.RoundDurationSeconds,
                RestDurationSeconds = request.Discipline.RestDurationSeconds,
                Scoring = new ScoringConfig
                {
                    VictoryPoints = request.Discipline.Scoring.VictoryPoints,
                    DrawPoints = request.Discipline.Scoring.DrawPoints,
                    DefeatPoints = request.Discipline.Scoring.DefeatPoints,
                    AllowKo = request.Discipline.Scoring.AllowKo,
                    AllowTko = request.Discipline.Scoring.AllowTko
                }
            }
        };

        db.Events.Add(entity);
        await db.SaveChangesAsync(ct);

        return TypedResults.Created($"/api/events/{entity.Id}", MapToResponse(entity));
    }

    private static async Task<Results<Ok<EventResponse>, NotFound>> Update(
        Guid id, UpdateEventRequest request, FmsDbContext db, CancellationToken ct)
    {
        var entity = await db.Events.FindAsync([id], ct);
        if (entity is null) return TypedResults.NotFound();

        entity.Name = request.Name;
        entity.Discipline.DisciplineName = request.Discipline.DisciplineName;
        entity.Discipline.TimerMode = request.Discipline.TimerMode;
        entity.Discipline.RoundCount = request.Discipline.RoundCount;
        entity.Discipline.RoundDurationSeconds = request.Discipline.RoundDurationSeconds;
        entity.Discipline.RestDurationSeconds = request.Discipline.RestDurationSeconds;
        entity.Discipline.Scoring.VictoryPoints = request.Discipline.Scoring.VictoryPoints;
        entity.Discipline.Scoring.DrawPoints = request.Discipline.Scoring.DrawPoints;
        entity.Discipline.Scoring.DefeatPoints = request.Discipline.Scoring.DefeatPoints;
        entity.Discipline.Scoring.AllowKo = request.Discipline.Scoring.AllowKo;
        entity.Discipline.Scoring.AllowTko = request.Discipline.Scoring.AllowTko;

        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(MapToResponse(entity));
    }

    private static async Task<Results<Ok<EventResponse>, NotFound, ValidationProblem>> UpdateStatus(
        Guid id, EventStatusUpdateRequest request, FmsDbContext db, CancellationToken ct)
    {
        var entity = await db.Events.FindAsync([id], ct);
        if (entity is null) return TypedResults.NotFound();

        // Basic status transition validation
        var valid = (entity.Status, request.Status) switch
        {
            (EventStatus.Draft, EventStatus.Active) => true,
            (EventStatus.Active, EventStatus.Completed) => true,
            _ => false
        };

        if (!valid)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["Status"] = [$"Cannot transition from {entity.Status} to {request.Status}"]
            });
        }

        entity.Status = request.Status;
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(MapToResponse(entity));
    }

    private static async Task<Results<Ok<EventSnapshotResponse>, NotFound>> GetSnapshot(
        Guid id, FmsDbContext db, CancellationToken ct)
    {
        var ev = await db.Events
            .AsNoTracking()
            .Include(e => e.Teams)
            .Include(e => e.Participants).ThenInclude(p => p.Team)
            .Include(e => e.Fights).ThenInclude(f => f.ParticipantA)
            .Include(e => e.Fights).ThenInclude(f => f.ParticipantB)
            .Include(e => e.Fights).ThenInclude(f => f.Result)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        if (ev is null) return TypedResults.NotFound();

        var snapshot = new EventSnapshotResponse(
            ev.Id,
            ev.Name,
            ev.Status,
            new DisciplineConfigDto(
                ev.Discipline.DisciplineName,
                ev.Discipline.TimerMode,
                ev.Discipline.RoundCount,
                ev.Discipline.RoundDurationSeconds,
                ev.Discipline.RestDurationSeconds,
                new ScoringConfigDto(
                    ev.Discipline.Scoring.VictoryPoints,
                    ev.Discipline.Scoring.DrawPoints,
                    ev.Discipline.Scoring.DefeatPoints,
                    ev.Discipline.Scoring.AllowKo,
                    ev.Discipline.Scoring.AllowTko)),
            ev.Teams.Select(t => new TeamResponse(t.Id, t.Name, t.EventId, t.Participants.Count, t.CreatedAt, t.UpdatedAt)).ToList(),
            ev.Participants.Select(p => new ParticipantResponse(p.Id, p.Name, p.Weight, p.EventId, p.TeamId, p.Team?.Name, p.CreatedAt, p.UpdatedAt)).ToList(),
            ev.Fights.OrderBy(f => f.OrderNumber).Select(f => new FightResponse(
                f.Id, f.EventId,
                f.ParticipantAId, f.ParticipantA.Name,
                f.ParticipantBId, f.ParticipantB.Name,
                f.OrderNumber, f.Status,
                f.Result != null ? new FightResultResponse(f.Result.Id, f.Result.Outcome, f.Result.Method, f.Result.Notes) : null,
                f.CreatedAt, f.UpdatedAt)).ToList());

        return TypedResults.Ok(snapshot);
    }

    private static EventResponse MapToResponse(Event e) => new(
        e.Id,
        e.Name,
        e.Status,
        new DisciplineConfigDto(
            e.Discipline.DisciplineName,
            e.Discipline.TimerMode,
            e.Discipline.RoundCount,
            e.Discipline.RoundDurationSeconds,
            e.Discipline.RestDurationSeconds,
            new ScoringConfigDto(
                e.Discipline.Scoring.VictoryPoints,
                e.Discipline.Scoring.DrawPoints,
                e.Discipline.Scoring.DefeatPoints,
                e.Discipline.Scoring.AllowKo,
                e.Discipline.Scoring.AllowTko)),
        e.CreatedAt,
        e.UpdatedAt);
}

public sealed record EventStatusUpdateRequest(EventStatus Status);
