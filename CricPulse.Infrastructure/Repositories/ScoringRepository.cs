using CricPulse.Infrastructure.Data;
using CricPulse.Domain.Entities;
using CricPulse.Application.Interfaces.Match;
using CricPulse.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CricPulse.Infrastructure.Repositories
{
    public class ScoringRepository : IScoringRepository
    {
        private readonly CricPulseDbContext _context;

        public ScoringRepository(CricPulseDbContext context)
        {
            _context = context;
        }

        // ================================================================
        // INNINGS / SCORING
        // ================================================================

        public async Task<Innings?> GetInningsForScoringAsync(int inningsId)
        {
            return await _context.Innings
                .Include(i => i.Match)
                    .ThenInclude(m => m.MatchPlayers)
                        .ThenInclude(mp => mp.Player)

                .Include(i => i.Match)
                    .ThenInclude(m => m.Innings)

                .Include(i => i.Balls
                    .OrderByDescending(b => b.Id))
                    .ThenInclude(b => b.Wicket)

                .FirstOrDefaultAsync(i => i.Id == inningsId);
        }

        public async Task AddBallAsync(Ball ball)
        {
            await _context.Balls.AddAsync(ball);
        }

        public async Task AddWicketAsync(Wicket wicket)
        {
            await _context.Wickets.AddAsync(wicket);
        }

        public Task RemoveBallAsync(Ball ball)
        {
            _context.Balls.Remove(ball);

            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }


        // ================================================================
        // MATCH / TOSS
        // ================================================================

        public async Task<Match?> GetMatchForTossAsync(int matchId)
        {
            return await _context.Matches
                .Include(m => m.MatchPlayers)
                .Include(m => m.Innings)
                .FirstOrDefaultAsync(m => m.Id == matchId);
        }

        public async Task<Match?> GetMatchForInningsAsync(int matchId)
        {
            return await _context.Matches
                .Include(m => m.MatchPlayers)
                    .ThenInclude(mp => mp.Player)

                .Include(m => m.Innings)
                    .ThenInclude(i => i.Balls)
                        .ThenInclude(b => b.Wicket)

                .FirstOrDefaultAsync(m => m.Id == matchId);
        }


        // ================================================================
        // MATCH LIFECYCLE
        // ================================================================

        public async Task<List<Match>> GetExpiredScheduledMatchesAsync()
        {
            var now = DateTime.UtcNow;

            return await _context.Matches
                .Where(m =>
                    m.Status == MatchStatus.Scheduled &&
                    m.MatchDate.Date < now.Date)
                .ToListAsync();
        }

        public async Task<List<Match>> GetPendingCompletionMatchesAsync()
        {
            var now = DateTime.UtcNow;

            return await _context.Matches
                .Where(m =>
                    m.Status == MatchStatus.PendingCompletion &&
                    m.CompletionDeadline != null &&
                    m.CompletionDeadline <= now)
                .ToListAsync();
        }


        // ================================================================
        // STATISTICS
        // ================================================================

        public async Task<Match?> GetMatchForStatisticsAsync(int matchId)
        {
            return await _context.Matches

                .Include(m => m.MatchPlayers)
                    .ThenInclude(mp => mp.Player)

                .Include(m => m.Innings)
                    .ThenInclude(i => i.Balls)
                        .ThenInclude(b => b.Wicket)

                .Include(m => m.Innings)
                    .ThenInclude(i => i.Balls)
                        .ThenInclude(b => b.StrikerMatchPlayer)

                .Include(m => m.Innings)
                    .ThenInclude(i => i.Balls)
                        .ThenInclude(b => b.BowlerMatchPlayer)

                .FirstOrDefaultAsync(m => m.Id == matchId);
        }


        // ================================================================
        // LEGACY / COMPATIBILITY
        // ================================================================

        public async Task DeleteCompletedMatchAsync(int matchId)
        {
            var match = await _context.Matches
                .FirstOrDefaultAsync(m => m.Id == matchId);

            if (match == null)
            {
                return;
            }

            _context.Matches.Remove(match);

            await _context.SaveChangesAsync();
        }
    }
}