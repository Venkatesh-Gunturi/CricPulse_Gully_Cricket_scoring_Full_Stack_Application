using CricPulse.Application.Interfaces.Player;
using CricPulse.Domain.Entities;
using CricPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CricPulse.Infrastructure.Repositories
{
    public class PlayerStatisticsRepository : IPlayerStatisticsRepository
    {
        private readonly CricPulseDbContext _context;

        public PlayerStatisticsRepository(CricPulseDbContext context)
        {
            _context = context;
        }

        public async Task<PlayerStatistics?> GetByPlayerIdAsync(
            int playerId)
        {
            return await _context.PlayerStatistics
                .FirstOrDefaultAsync(
                    ps => ps.PlayerId == playerId);
        }

        public async Task<PlayerStatistics> CreateAsync(
            PlayerStatistics statistics)
        {
            await _context.PlayerStatistics.AddAsync(
                statistics);

            await _context.SaveChangesAsync();

            return statistics;
        }

        public async Task<PlayerStatistics> UpdateAsync(
            PlayerStatistics statistics)
        {
            _context.PlayerStatistics.Update(statistics);

            await _context.SaveChangesAsync();

            return statistics;
        }
    }
}