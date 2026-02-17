using FMS.Application.DTOs;
using FMS.Domain.Entities;
using FMS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace FMS.Api.Endpoints;

public static class TeamEndpoints
{
    public static RouteGroupBuilder MapTeamEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/events/{eventId:guid}/teams").WithTags("teams");

        group.MapGet("/", GetAll);
        group.MapGet("/{id:guid}", GetById);
        group.MapPost("/", Create);
        group.MapPut("/{id:guid}", Update);
        group.MapDelete("/{id:guid}", Delete);

        return group;
    }

    private static async Task<Results<Ok<List<TeamResponse>>, NotFound>> GetAll(
        Guid eventId, FmsDbContext db, CancellationToken ct)
    {
        var eventExists = await db.Events.AnyAsync(e => e.Id == eventId, ct);
        if (!eventExists) return TypedResults.NotFound();

        var teams = await db.Teams
            .AsNoTracking()
            .Where(t => t.EventId == eventId)
            .Select(t => new TeamResponse(t.Id, t.Name, t.EventId, t.Participants.Count, t.CreatedAt, t.UpdatedAt))
            .ToListAsync(ct);

        return TypedResults.Ok(teams);
    }

    private static async Task<Results<Ok<TeamResponse>, NotFound>> GetById(
        Guid eventId, Guid id, FmsDbContext db, CancellationToken ct)
    {
        var team = await db.Teams
            .AsNoTracking()
            .Where(t => t.Id == id && t.EventId == eventId)
            .Select(t => new TeamResponse(t.Id, t.Name, t.EventId, t.Participants.Count, t.CreatedAt, t.UpdatedAt))
            .FirstOrDefaultAsync(ct);

        return team is not null
            ? TypedResults.Ok(team)
            : TypedResults.NotFound();
    }

    private static async Task<Results<Created<TeamResponse>, NotFound>> Create(
        Guid eventId, CreateTeamRequest request, FmsDbContext db, CancellationToken ct)
    {
        var eventExists = await db.Events.AnyAsync(e => e.Id == eventId, ct);
        if (!eventExists) return TypedResults.NotFound();

        var entity = new Team
        {
            Name = request.Name,
            EventId = eventId
        };

        db.Teams.Add(entity);
        await db.SaveChangesAsync(ct);

        var response = new TeamResponse(entity.Id, entity.Name, entity.EventId, 0, entity.CreatedAt, entity.UpdatedAt);
        return TypedResults.Created($"/api/events/{eventId}/teams/{entity.Id}", response);
    }

    private static async Task<Results<Ok<TeamResponse>, NotFound>> Update(
        Guid eventId, Guid id, UpdateTeamRequest request, FmsDbContext db, CancellationToken ct)
    {
        var entity = await db.Teams.FirstOrDefaultAsync(t => t.Id == id && t.EventId == eventId, ct);
        if (entity is null) return TypedResults.NotFound();

        entity.Name = request.Name;
        await db.SaveChangesAsync(ct);

        var count = await db.Participants.CountAsync(p => p.TeamId == id, ct);
        return TypedResults.Ok(new TeamResponse(entity.Id, entity.Name, entity.EventId, count, entity.CreatedAt, entity.UpdatedAt));
    }

    private static async Task<Results<NoContent, NotFound>> Delete(
        Guid eventId, Guid id, FmsDbContext db, CancellationToken ct)
    {
        var entity = await db.Teams.FirstOrDefaultAsync(t => t.Id == id && t.EventId == eventId, ct);
        if (entity is null) return TypedResults.NotFound();

        db.Teams.Remove(entity);
        await db.SaveChangesAsync(ct);

        return TypedResults.NoContent();
    }
}
