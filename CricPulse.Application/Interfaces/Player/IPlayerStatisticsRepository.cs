using CricPulse.Domain.Entities;

namespace CricPulse.Application.Interfaces.Player
{
    public interface IPlayerStatisticsRepository
    {
        Task<PlayerStatistics?> GetByPlayerIdAsync(int playerId);

        Task<PlayerStatistics> CreateAsync(
            PlayerStatistics statistics);

        Task<PlayerStatistics> UpdateAsync(
            PlayerStatistics statistics);
    }
}