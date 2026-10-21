using CricPulse.Domain.Entities;

namespace CricPulse.Application.Interfaces.Repositories;

public interface IPlayerStatisticsRepository
{
    Task<PlayerStatistics?> GetByUserIdAsync(Guid userId);

    Task AddAsync(PlayerStatistics statistics);

    Task UpdateAsync(PlayerStatistics statistics);
}