using CricPulse.Application.Interfaces.Repositories;
using CricPulse.Domain.Entities;
using CricPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CricPulse.Infrastructure.Repositories;

public class MatchRepository : RepositoryBase<Match>, IMatchRepository
{
    public MatchRepository(CricPulseDbContext context)
        : base(context)
    {
    }

    public async Task<Match?> GetByIdAsync(Guid matchId)
    {
        return await DbSet
            .Include(x => x.CreatedByUser)
            .Include(x => x.MatchPlayers)
                .ThenInclude(x => x.User)
            .Include(x => x.Innings)
                .ThenInclude(x => x.Balls)
                    .ThenInclude(x => x.Wicket)
            .FirstOrDefaultAsync(x => x.Id == matchId);
    }

    public async Task AddAsync(Match match)
    {
        await AddEntityAsync(match);
    }

    public Task UpdateAsync(Match match)
    {
        UpdateEntity(match);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<Match>> GetByStateAsync(string state)
    {
        return await DbSet
            .AsNoTracking()
            .Where(x => x.State != null && x.State == state)
            .OrderBy(x => x.ScheduledAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Match>> GetByStatusAsync(string status)
    {
        return await DbSet
            .AsNoTracking()
            .Where(x => x.Status == status)
            .OrderBy(x => x.ScheduledAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Match>> GetForDiscoveryAsync(
        string? status,
        string? state,
        double? minLatitude,
        double? maxLatitude,
        double? minLongitude,
        double? maxLongitude)
    {
        IQueryable<Match> query = DbSet
            .AsNoTracking()
            .Include(x => x.Innings);

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(state))
        {
            query = query.Where(
                x => x.State != null && x.State == state);
        }

        if (minLatitude.HasValue && maxLatitude.HasValue)
        {
            query = query.Where(x =>
                x.Latitude >= minLatitude.Value &&
                x.Latitude <= maxLatitude.Value);
        }

        if (minLongitude.HasValue && maxLongitude.HasValue)
        {
            query = query.Where(x =>
                x.Longitude >= minLongitude.Value &&
                x.Longitude <= maxLongitude.Value);
        }

        return await query
            .OrderBy(x => x.ScheduledAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Match>> GetNearestAsync(
        double latitude,
        double longitude,
        int count)
    {
        // A fast geographic pre-filter. Exact distance should be
        // calculated with the Haversine formula in the application layer.
        var latitudeDelta = 30d / 110.574d;
        var longitudeDelta =
            30d / (111.320d * Math.Max(
                0.01d,
                Math.Abs(Math.Cos(latitude * Math.PI / 180d))));

        return await DbSet
            .AsNoTracking()
            .Where(x =>
                x.Latitude >= latitude - latitudeDelta &&
                x.Latitude <= latitude + latitudeDelta &&
                x.Longitude >= longitude - longitudeDelta &&
                x.Longitude <= longitude + longitudeDelta)
            .Take(count)
            .ToListAsync();
    }


    public async Task<IReadOnlyList<Match>> GetByCreatorAsync(
        Guid createdByUserId,
        string? status)
    {
        IQueryable<Match> query = DbSet
            .AsNoTracking()
            .Where(x => x.CreatedByUserId == createdByUserId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

        return await query
            .OrderBy(x => x.ScheduledAt)
            .ToListAsync();
    }


    public async Task DeleteAsync(Match match)
    {
        // Load dependent records so they can be deleted in a safe order.
        var innings = await Context.Set<Innings>()
            .Where(x => x.MatchId == match.Id)
            .ToListAsync();

        var inningsIds = innings.Select(x => x.Id).ToList();

        var balls = inningsIds.Count == 0
            ? new List<Ball>()
            : await Context.Set<Ball>()
                .Where(x => inningsIds.Contains(x.InningsId))
                .ToListAsync();

        var ballIds = balls.Select(x => x.Id).ToList();

        var wickets = ballIds.Count == 0
            ? new List<Wicket>()
            : await Context.Set<Wicket>()
                .Where(x => ballIds.Contains(x.BallId))
                .ToListAsync();

        var matchPlayers = await Context.Set<MatchPlayer>()
            .Where(x => x.MatchId == match.Id)
            .ToListAsync();

        // Remove dependents before the records they reference.
        Context.Set<Wicket>().RemoveRange(wickets);
        Context.Set<Ball>().RemoveRange(balls);
        Context.Set<Innings>().RemoveRange(innings);
        Context.Set<MatchPlayer>().RemoveRange(matchPlayers);

        // Reuse an already tracked instance if one exists.
        var trackedMatch = Context.Set<Match>()
            .Local
            .FirstOrDefault(x => x.Id == match.Id);

        Context.Set<Match>().Remove(trackedMatch ?? match);
    }


}