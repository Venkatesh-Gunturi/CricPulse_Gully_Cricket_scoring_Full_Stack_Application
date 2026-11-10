using CricPulse.Application.Interfaces.Repositories;
using CricPulse.Domain.Entities;
using CricPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CricPulse.Infrastructure.Repositories;
// Handles database operations required for match scoring.
public class MatchScoringRepository : IMatchScoringRepository
{
    private readonly CricPulseDbContext _context;

    public MatchScoringRepository(CricPulseDbContext context)
    {
        _context = context;
    }

    // Retrieves an innings with its match details, balls, and associated wickets
    // required for scoring operations.
    public async Task<Innings?> GetInningsForScoringAsync(Guid inningsId)
    {
        return await _context.Innings
            .Include(x => x.Match)
            .Include(x => x.Balls)
                .ThenInclude(x => x.Wicket)
            .FirstOrDefaultAsync(x => x.Id == inningsId);
    }

    // Retrieves the most recently created ball in the specified innings,
    // including its associated wicket.
    public async Task<Ball?> GetLastBallAsync(Guid inningsId)
    {
        return await _context.Balls
            .Include(x => x.Wicket)
            .Where(x => x.InningsId == inningsId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();
    }

    // Adds a new ball to the current database context.
    public async Task AddBallAsync(Ball ball)
    {
        await _context.Balls.AddAsync(ball);
    }

    // Adds a wicket record to the current database context.
    public async Task AddWicketAsync(Wicket wicket)
    {
        await _context.Wickets.AddAsync(wicket);
    }

    // Marks the specified ball for deletion from the database.
    public Task DeleteBallAsync(Ball ball)
    {
        _context.Balls.Remove(ball);
        return Task.CompletedTask;
    }

    // Marks the innings entity as modified so its changes can be persisted.
    public Task UpdateInningsAsync(Innings innings)
    {
        _context.Innings.Update(innings);
        return Task.CompletedTask;
    }

    // Persists all pending scoring changes to the database.
    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}