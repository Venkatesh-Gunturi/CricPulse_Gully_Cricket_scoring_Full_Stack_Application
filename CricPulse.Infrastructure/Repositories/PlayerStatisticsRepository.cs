using CricPulse.Application.Interfaces.Player;
using CricPulse.Domain.Entities;
using CricPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CricPulse.Infrastructure.Repositories
{
    public class PlayerStatisticsRepository : IPlayerStatisticsRepository
    {
        private readonly CricPulseDbContext _context;

        public PlayerStatisticsRepository(
            CricPulseDbContext context)
        {
            _context = context;
        }

        // Purpose:
        // Load the existing statistics record for a player so accumulated
        // career statistics can be updated when a match is completed.
        public async Task<PlayerStatistics?> GetByPlayerIdAsync(
            int playerId)
        {
            return await _context.PlayerStatistics
                .FirstOrDefaultAsync(
                    ps => ps.PlayerId == playerId);
        }

        // Purpose:
        // Create the initial statistics record for a player who does not
        // yet have one when completed-match statistics are aggregated.
        public async Task<PlayerStatistics> CreateAsync(
            PlayerStatistics statistics)
        {
            await _context.PlayerStatistics.AddAsync(statistics);

            return statistics;
        }

        // Purpose:
        // Persist updated career statistics for an existing player.
        public async Task<PlayerStatistics> UpdateAsync(
            PlayerStatistics statistics)
        {
            _context.PlayerStatistics.Update(statistics);

            await Task.CompletedTask;

            return statistics;
        }
    }
}