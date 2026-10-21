using CricPulse.Domain.Entities;

namespace CricPulse.Application.Interfaces.Repositories;

public interface IOrganizerStatisticsRepository
{
    Task<OrganizerStatistics?> GetByUserIdAsync(Guid userId);

    Task AddAsync(OrganizerStatistics statistics);

    Task UpdateAsync(OrganizerStatistics statistics);
}