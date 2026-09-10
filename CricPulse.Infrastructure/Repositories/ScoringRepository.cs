using CricPulse.Application.Interfaces.Match;
using CricPulse.Domain.Entities;
using CricPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using BallEntity = CricPulse.Domain.Entities.Ball;
using InningsEntity=CricPulse.Domain.Entities.Innings;
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
    }
}