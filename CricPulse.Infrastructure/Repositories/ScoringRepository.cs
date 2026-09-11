using CricPulse.Application.Interfaces.Match;
using CricPulse.Domain.Entities;
using CricPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using BallEntity = CricPulse.Domain.Entities.Ball;
using InningsEntity=CricPulse.Domain.Entities.Innings;
using WicketEntity = CricPulse.Domain.Entities.Wicket;
using MatchEntity = CricPulse.Domain.Entities.Match;

namespace CricPulse.Infrastructure.Repositories.Match
{
    public class ScoringRepository : IScoringRepository
    {
        private readonly CricPulseDbContext _context;

        public ScoringRepository(CricPulseDbContext context)
        {
            _context = context;
        }

        public async Task<InningsEntity?> GetInningsForScoringAsync(int inningsId)
        {
            return await _context.Innings
            .Include(i => i.Match)
            .Include(i => i.Balls)
                .ThenInclude(b => b.Wicket)
            .FirstOrDefaultAsync(i => i.Id == inningsId);
        }

        public async Task AddBallAsync(BallEntity ball)
        {
            await _context.Balls.AddAsync(ball);
        }

        public async Task AddWicketAsync(WicketEntity wicket)
        {
            await _context.Wickets.AddAsync(wicket);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        // Purpose:
        // Remove the scoring action and its associated wicket, if one exists,
        // so an undo can completely reverse the latest delivery.
        public async Task RemoveBallAsync(Ball ball)
        {
            if (ball.Wicket != null)
            {
                _context.Wickets.Remove(ball.Wicket);
            }

            _context.Balls.Remove(ball);

            await Task.CompletedTask;
        }

        // Purpose:
        // Load the match and its innings so toss operations can verify
        // whether the innings has already started.
        public async Task<MatchEntity?> GetMatchForTossAsync(int matchId)
        {
            return await _context.Matches
                .Include(m => m.Innings)
                .FirstOrDefaultAsync(m => m.Id == matchId);
        }


        // Purpose:
        // Find scheduled matches whose 24-hour start window has expired
        // so they can be automatically cancelled.
        public async Task<List<MatchEntity>> GetExpiredScheduledMatchesAsync()
        {
            var now = DateTime.UtcNow;

            return await _context.Matches
                .Where(m => m.Status == "Scheduled")
                .Where(m => m.MatchDate.Date.Add(m.MatchTime).AddHours(24) < now)
                .ToListAsync();
        }

        // Purpose:
        // Load the match together with its players so innings setup can validate
        // the selected striker, non-striker, and bowler.
        public async Task<MatchEntity?> GetMatchForInningsAsync(int matchId)
        {
            return await _context.Matches
                .Include(m => m.MatchPlayers)
                .FirstOrDefaultAsync(m => m.Id == matchId);
        }
    }
}