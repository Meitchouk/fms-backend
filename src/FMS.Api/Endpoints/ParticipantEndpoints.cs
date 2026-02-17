using FMS.Application.DTOs;
using FMS.Domain.Entities;
using FMS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace FMS.Api.Endpoints;

public static class ParticipantEndpoints
{
    public static RouteGroupBuilder MapParticipantEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/events/{eventId:guid}/participants").WithTags("participants");

        group.MapGet("/", GetAll);
        group.MapGet("/{id:guid}", GetById);
        group.MapPost("/", Create);
        group.MapPut("/{id:guid}", Update);
        group.MapDelete("/{id:guid}", Delete);

        return group;
    }

    private static async Task<Results<Ok<List<ParticipantResponse>>, NotFound>> GetAll(
        Guid eventId, FmsDbContext db, CancellationToken ct)
    {
        var eventExists = await db.Events.AnyAsync(e => e.Id == eventId, ct);
        if (!eventExists) return TypedResults.NotFound();

        var participants = await db.Participants
            .AsNoTracking()
            .Where(p => p.EventId == eventId)
            .Include(p => p.Team)
            .Select(p => new ParticipantResponse(p.Id, p.Name, p.Weight, p.EventId, p.TeamId, p.Team != null ? p.Team.Name : null, p.CreatedAt, p.UpdatedAt))
            .ToListAsync(ct);

        return TypedResults.Ok(participants);
    }

    private static async Task<Results<Ok<ParticipantResponse>, NotFound>> GetById(
        Guid eventId, Guid id, FmsDbContext db, CancellationToken ct)
    {
        var participant = await db.Participants
            .AsNoTracking()
            .Where(p => p.Id == id && p.EventId == eventId)
            .Include(p => p.Team)
            .Select(p => new ParticipantResponse(p.Id, p.Name, p.Weight, p.EventId, p.TeamId, p.Team != null ? p.Team.Name : null, p.CreatedAt, p.UpdatedAt))
            .FirstOrDefaultAsync(ct);

        return participant is not null
            ? TypedResults.Ok(participant)
            : TypedResults.NotFound();
    }

    private static async Task<Results<Created<ParticipantResponse>, NotFound, ValidationProblem>> Create(
        Guid eventId, CreateParticipantRequest request, FmsDbContext db, CancellationToken ct)
    {
        var eventExists = await db.Events.AnyAsync(e => e.Id == eventId, ct);
        if (!eventExists) return TypedResults.NotFound();

        // Validate team belongs to same event if specified
        if (request.TeamId.HasValue)
        {
            var teamExists = await db.Teams.AnyAsync(t => t.Id == request.TeamId.Value && t.EventId == eventId, ct);
            if (!teamExists)
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["TeamId"] = ["Team does not exist in this event"]
                });
            }
        }

        var entity = new Participant
        {
            Name = request.Name,
            Weight = request.Weight,
            EventId = eventId,
            TeamId = request.TeamId
        };

        db.Participants.Add(entity);
        await db.SaveChangesAsync(ct);

        string? teamName = null;
        if (request.TeamId.HasValue)
        {
            teamName = await db.Teams.Where(t => t.Id == request.TeamId.Value).Select(t => t.Name).FirstOrDefaultAsync(ct);
        }

        var response = new ParticipantResponse(entity.Id, entity.Name, entity.Weight, entity.EventId, entity.TeamId, teamName, entity.CreatedAt, entity.UpdatedAt);
        return TypedResults.Created($"/api/events/{eventId}/participants/{entity.Id}", response);
    }

    private static async Task<Results<Ok<ParticipantResponse>, NotFound, ValidationProblem>> Update(
        Guid eventId, Guid id, UpdateParticipantRequest request, FmsDbContext db, CancellationToken ct)
    {
        var entity = await db.Participants.FirstOrDefaultAsync(p => p.Id == id && p.EventId == eventId, ct);
        if (entity is null) return TypedResults.NotFound();

        // Validate team belongs to same event if specified
        if (request.TeamId.HasValue)
        {
            var teamExists = await db.Teams.AnyAsync(t => t.Id == request.TeamId.Value && t.EventId == eventId, ct);
            if (!teamExists)
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["TeamId"] = ["Team does not exist in this event"]
                });
            }
        }

        entity.Name = request.Name;
        entity.Weight = request.Weight;
        entity.TeamId = request.TeamId;

        await db.SaveChangesAsync(ct);

        string? teamName = null;
        if (entity.TeamId.HasValue)
        {
            teamName = await db.Teams.Where(t => t.Id == entity.TeamId.Value).Select(t => t.Name).FirstOrDefaultAsync(ct);
        }

        return TypedResults.Ok(new ParticipantResponse(entity.Id, entity.Name, entity.Weight, entity.EventId, entity.TeamId, teamName, entity.CreatedAt, entity.UpdatedAt));
    }

    private static async Task<Results<NoContent, NotFound, ValidationProblem>> Delete(
        Guid eventId, Guid id, FmsDbContext db, CancellationToken ct)
    {
        var entity = await db.Participants.FirstOrDefaultAsync(p => p.Id == id && p.EventId == eventId, ct);
        if (entity is null) return TypedResults.NotFound();

        // Check if participant is in any scheduled fight
        var inFight = await db.Fights.AnyAsync(f =>
            f.EventId == eventId &&
            (f.ParticipantAId == id || f.ParticipantBId == id) &&
            f.Status != Domain.Enums.FightStatus.Cancelled, ct);

        if (inFight)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["Participant"] = ["Cannot delete a participant who is assigned to active fights. Cancel the fights first."]
            });
        }

        db.Participants.Remove(entity);
        await db.SaveChangesAsync(ct);

        return TypedResults.NoContent();
    }
}
