using FMS.Application.DTOs;
using FMS.Domain.Entities;
using FMS.Domain.Enums;
using FMS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace FMS.Api.Endpoints;

public static class FightEndpoints
{
    public static RouteGroupBuilder MapFightEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/events/{eventId:guid}/fights").WithTags("fights");

        group.MapGet("/", GetAll);
        group.MapGet("/{id:guid}", GetById);
        group.MapPost("/", Create);
        group.MapPut("/{id:guid}", Update);
        group.MapDelete("/{id:guid}", Delete);
        group.MapPut("/reorder", Reorder);

        return group;
    }

    private static async Task<Results<Ok<List<FightResponse>>, NotFound>> GetAll(
        Guid eventId, FmsDbContext db, CancellationToken ct)
    {
        var eventExists = await db.Events.AnyAsync(e => e.Id == eventId, ct);
        if (!eventExists) return TypedResults.NotFound();

        var fights = await db.Fights
            .AsNoTracking()
            .Where(f => f.EventId == eventId)
            .Include(f => f.ParticipantA)
            .Include(f => f.ParticipantB)
            .Include(f => f.Result)
            .OrderBy(f => f.OrderNumber)
            .Select(f => MapToResponse(f))
            .ToListAsync(ct);

        return TypedResults.Ok(fights);
    }

    private static async Task<Results<Ok<FightResponse>, NotFound>> GetById(
        Guid eventId, Guid id, FmsDbContext db, CancellationToken ct)
    {
        var fight = await db.Fights
            .AsNoTracking()
            .Where(f => f.Id == id && f.EventId == eventId)
            .Include(f => f.ParticipantA)
            .Include(f => f.ParticipantB)
            .Include(f => f.Result)
            .FirstOrDefaultAsync(ct);

        return fight is not null
            ? TypedResults.Ok(MapToResponse(fight))
            : TypedResults.NotFound();
    }

    private static async Task<Results<Created<FightResponse>, NotFound, ValidationProblem>> Create(
        Guid eventId, CreateFightRequest request, FmsDbContext db, CancellationToken ct)
    {
        var ev = await db.Events.FindAsync([eventId], ct);
        if (ev is null) return TypedResults.NotFound();

        // Validate participants belong to the same event
        var participantA = await db.Participants.FirstOrDefaultAsync(p => p.Id == request.ParticipantAId && p.EventId == eventId, ct);
        var participantB = await db.Participants.FirstOrDefaultAsync(p => p.Id == request.ParticipantBId && p.EventId == eventId, ct);

        var errors = new Dictionary<string, string[]>();

        if (participantA is null)
            errors["ParticipantAId"] = ["Participant A does not exist in this event"];
        if (participantB is null)
            errors["ParticipantBId"] = ["Participant B does not exist in this event"];
        if (request.ParticipantAId == request.ParticipantBId)
            errors["Participants"] = ["A participant cannot fight themselves"];

        if (errors.Count > 0)
            return TypedResults.ValidationProblem(errors);

        // Check for duplicate order number
        var orderExists = await db.Fights.AnyAsync(f => f.EventId == eventId && f.OrderNumber == request.OrderNumber, ct);
        if (orderExists)
        {
            errors["OrderNumber"] = [$"Order number {request.OrderNumber} is already taken"];
            return TypedResults.ValidationProblem(errors);
        }

        var entity = new Fight
        {
            EventId = eventId,
            ParticipantAId = request.ParticipantAId,
            ParticipantBId = request.ParticipantBId,
            OrderNumber = request.OrderNumber,
            Status = FightStatus.Scheduled
        };

        db.Fights.Add(entity);
        await db.SaveChangesAsync(ct);

        // Reload with navigation props
        var created = await db.Fights
            .AsNoTracking()
            .Include(f => f.ParticipantA)
            .Include(f => f.ParticipantB)
            .FirstAsync(f => f.Id == entity.Id, ct);

        return TypedResults.Created($"/api/events/{eventId}/fights/{entity.Id}", MapToResponse(created));
    }

    private static async Task<Results<Ok<FightResponse>, NotFound, ValidationProblem>> Update(
        Guid eventId, Guid id, UpdateFightRequest request, FmsDbContext db, CancellationToken ct)
    {
        var entity = await db.Fights
            .Include(f => f.ParticipantA)
            .Include(f => f.ParticipantB)
            .FirstOrDefaultAsync(f => f.Id == id && f.EventId == eventId, ct);

        if (entity is null) return TypedResults.NotFound();

        // Can only update scheduled fights
        if (entity.Status != FightStatus.Scheduled)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["Status"] = [$"Cannot update a fight with status {entity.Status}"]
            });
        }

        var errors = new Dictionary<string, string[]>();

        var participantA = await db.Participants.FirstOrDefaultAsync(p => p.Id == request.ParticipantAId && p.EventId == eventId, ct);
        var participantB = await db.Participants.FirstOrDefaultAsync(p => p.Id == request.ParticipantBId && p.EventId == eventId, ct);

        if (participantA is null)
            errors["ParticipantAId"] = ["Participant A does not exist in this event"];
        if (participantB is null)
            errors["ParticipantBId"] = ["Participant B does not exist in this event"];
        if (request.ParticipantAId == request.ParticipantBId)
            errors["Participants"] = ["A participant cannot fight themselves"];

        if (errors.Count > 0)
            return TypedResults.ValidationProblem(errors);

        entity.ParticipantAId = request.ParticipantAId;
        entity.ParticipantBId = request.ParticipantBId;

        await db.SaveChangesAsync(ct);

        var updated = await db.Fights
            .AsNoTracking()
            .Include(f => f.ParticipantA)
            .Include(f => f.ParticipantB)
            .Include(f => f.Result)
            .FirstAsync(f => f.Id == id, ct);

        return TypedResults.Ok(MapToResponse(updated));
    }

    private static async Task<Results<NoContent, NotFound, ValidationProblem>> Delete(
        Guid eventId, Guid id, FmsDbContext db, CancellationToken ct)
    {
        var entity = await db.Fights.FirstOrDefaultAsync(f => f.Id == id && f.EventId == eventId, ct);
        if (entity is null) return TypedResults.NotFound();

        if (entity.Status != FightStatus.Scheduled && entity.Status != FightStatus.Cancelled)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["Status"] = [$"Cannot delete a fight with status {entity.Status}"]
            });
        }

        db.Fights.Remove(entity);
        await db.SaveChangesAsync(ct);

        return TypedResults.NoContent();
    }

    private static async Task<Results<Ok<List<FightResponse>>, NotFound, ValidationProblem>> Reorder(
        Guid eventId, ReorderFightsRequest request, FmsDbContext db, CancellationToken ct)
    {
        var eventExists = await db.Events.AnyAsync(e => e.Id == eventId, ct);
        if (!eventExists) return TypedResults.NotFound();

        var fights = await db.Fights
            .Where(f => f.EventId == eventId)
            .ToListAsync(ct);

        var errors = new Dictionary<string, string[]>();

        // Validate: no in-progress fights being reordered
        foreach (var item in request.Fights)
        {
            var fight = fights.FirstOrDefault(f => f.Id == item.FightId);
            if (fight is null)
            {
                errors[item.FightId.ToString()] = ["Fight not found in this event"];
                continue;
            }

            if (fight.Status == FightStatus.InProgress)
            {
                errors[item.FightId.ToString()] = ["Cannot reorder an in-progress fight"];
            }
        }

        // Validate: no duplicate order numbers
        var orderNumbers = request.Fights.Select(f => f.NewOrderNumber).ToList();
        if (orderNumbers.Distinct().Count() != orderNumbers.Count)
        {
            errors["OrderNumbers"] = ["Duplicate order numbers are not allowed"];
        }

        if (errors.Count > 0)
            return TypedResults.ValidationProblem(errors);

        // Two-phase reorder to avoid unique constraint conflicts:
        // Phase 1: set all order numbers to negative temporaries
        foreach (var item in request.Fights)
        {
            var fight = fights.First(f => f.Id == item.FightId);
            fight.OrderNumber = -item.NewOrderNumber;
        }
        await db.SaveChangesAsync(ct);

        // Phase 2: set to final positive values
        foreach (var item in request.Fights)
        {
            var fight = fights.First(f => f.Id == item.FightId);
            fight.OrderNumber = item.NewOrderNumber;
        }
        await db.SaveChangesAsync(ct);

        // Return updated fight list
        var result = await db.Fights
            .AsNoTracking()
            .Where(f => f.EventId == eventId)
            .Include(f => f.ParticipantA)
            .Include(f => f.ParticipantB)
            .Include(f => f.Result)
            .OrderBy(f => f.OrderNumber)
            .Select(f => MapToResponse(f))
            .ToListAsync(ct);

        return TypedResults.Ok(result);
    }

    private static FightResponse MapToResponse(Fight f) => new(
        f.Id,
        f.EventId,
        f.ParticipantAId,
        f.ParticipantA.Name,
        f.ParticipantBId,
        f.ParticipantB.Name,
        f.OrderNumber,
        f.Status,
        f.Result != null
            ? new FightResultResponse(f.Result.Id, f.Result.Outcome, f.Result.Method, f.Result.Notes)
            : null,
        f.CreatedAt,
        f.UpdatedAt);
}
