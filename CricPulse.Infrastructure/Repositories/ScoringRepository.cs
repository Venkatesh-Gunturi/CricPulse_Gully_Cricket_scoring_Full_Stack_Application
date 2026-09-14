using CricPulse.Application.Interfaces.Match;
using CricPulse.Application.Utilities;
using CricPulse.Domain.Entities;
using CricPulse.Domain.Enums;
using CricPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using BallEntity = CricPulse.Domain.Entities.Ball;
using InningsEntity=CricPulse.Domain.Entities.Innings;
using MatchEntity = CricPulse.Domain.Entities.Match;
using WicketEntity = CricPulse.Domain.Entities.Wicket;

namespace CricPulse.Infrastructure.Repositories.Match
{
    public class ScoringRepository : IScoringRepository
    {
        private readonly CricPulseDbContext _context;

        public ScoringRepository(CricPulseDbContext context)
        {
            _context = context;
        }

        // Purpose:
        // Load the innings together with its match, all match innings, and ball history
        // so scoring operations can correctly determine targets, results, and undo state.
        public async Task<InningsEntity?> GetInningsForScoringAsync(
            int inningsId)
        {
            return await _context.Innings
                .Include(i => i.Match)
                    .ThenInclude(m => m.Innings)
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
        // Find scheduled matches whose 24-hour start window has expired by calculating
        // the scheduled IST time in application code so the query remains SQL-compatible.
        public async Task<List<MatchEntity>> GetExpiredScheduledMatchesAsync()
        {
            var now = DateTime.UtcNow;

            var scheduledMatches = await _context.Matches
                .Where(m => m.Status == MatchStatus.Scheduled)
                .ToListAsync();

            return scheduledMatches
                .Where(m =>
                    MatchTimeHelper
                        .GetScheduledUtc(
                            m.MatchDate,
                            m.MatchTime)
                        .AddHours(24) < now)
                .ToList();
        }

        // Purpose:
        // Load the match together with its players and existing innings so innings
        // setup can validate the selected players and determine the correct innings number.
        public async Task<MatchEntity?> GetMatchForInningsAsync(
            int matchId)
        {
            return await _context.Matches
                .Include(m => m.MatchPlayers)
                .Include(m => m.Innings)
                .FirstOrDefaultAsync(m => m.Id == matchId);
        }


        // Purpose:
        // Find matches whose final-result confirmation window has expired so the
        // background process can automatically complete them.
        public async Task<List<MatchEntity>> GetPendingCompletionMatchesAsync()
        {
            var now = DateTime.UtcNow;

            return await _context.Matches
                .Where(m =>
                    m.Status == MatchStatus.PendingCompletion &&
                    m.CompletionDeadline != null &&
                    m.CompletionDeadline <= now)
                .ToListAsync();
        }

        // Purpose:
        // Load the complete scoring history and match lineup required to calculate
        // player statistics before the completed match data is permanently deleted.
        public async Task<MatchEntity?> GetMatchForStatisticsAsync(
            int matchId)
        {
            return await _context.Matches
                .Include(m => m.MatchPlayers)
                .Include(m => m.Innings)
                    .ThenInclude(i => i.Balls)
                        .ThenInclude(b => b.Wicket)
                .FirstOrDefaultAsync(m => m.Id == matchId);
        }


        // Purpose:
        // Permanently remove a completed match and its scoring records after player
        // statistics have already been aggregated.
        public async Task DeleteCompletedMatchAsync(int matchId)
        {
            var match = await _context.Matches
                .Include(m => m.MatchPlayers)
                .Include(m => m.Innings)
                    .ThenInclude(i => i.Balls)
                        .ThenInclude(b => b.Wicket)
                .FirstOrDefaultAsync(m => m.Id == matchId);

            if (match == null)
            {
                return;
            }

            foreach (var innings in match.Innings)
            {
                foreach (var ball in innings.Balls)
                {
                    if (ball.Wicket != null)
                    {
                        _context.Wickets.Remove(ball.Wicket);
                    }
                }

                _context.Balls.RemoveRange(innings.Balls);
            }

            _context.Innings.RemoveRange(match.Innings);
            _context.MatchPlayers.RemoveRange(match.MatchPlayers);
            _context.Matches.Remove(match);
        }
    }
}