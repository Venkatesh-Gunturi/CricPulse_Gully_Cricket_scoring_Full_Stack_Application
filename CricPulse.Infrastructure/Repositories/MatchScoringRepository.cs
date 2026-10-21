using CricPulse.Application.Interfaces.Repositories;
using CricPulse.Domain.Entities;
using CricPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CricPulse.Infrastructure.Repositories;

public class MatchScoringRepository : IMatchScoringRepository
{
    private readonly CricPulseDbContext _context;

    public MatchScoringRepository(CricPulseDbContext context)
    {
        _context = context;
    }

    public async Task<Innings?> GetInningsForScoringAsync(Guid inningsId)
    {
        return await _context.Innings
            .Include(x => x.Match)
            .Include(x => x.Balls)
                .ThenInclude(x => x.Wicket)
            .FirstOrDefaultAsync(x => x.Id == inningsId);
    }

    public async Task<Ball?> GetLastBallAsync(Guid inningsId)
    {
        return await _context.Balls
            .Include(x => x.Wicket)
            .Where(x => x.InningsId == inningsId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task AddBallAsync(Ball ball)
    {
        await _context.Balls.AddAsync(ball);
    }

    public async Task AddWicketAsync(Wicket wicket)
    {
        await _context.Wickets.AddAsync(wicket);
    }

    public Task DeleteBallAsync(Ball ball)
    {
        _context.Balls.Remove(ball);
        return Task.CompletedTask;
    }

    public Task UpdateInningsAsync(Innings innings)
    {
        _context.Innings.Update(innings);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}