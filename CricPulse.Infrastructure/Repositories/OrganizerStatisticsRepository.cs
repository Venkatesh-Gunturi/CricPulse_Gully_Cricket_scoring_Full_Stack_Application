using CricPulse.Application.Interfaces.Repositories;
using CricPulse.Domain.Entities;
using CricPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CricPulse.Infrastructure.Repositories;
// Handles database operations for OrganizerStatistics entities.
public class OrganizerStatisticsRepository : RepositoryBase<OrganizerStatistics>, IOrganizerStatisticsRepository
{
    public OrganizerStatisticsRepository(CricPulseDbContext context) : base(context)
    {
    }
    // Retrieves an organizer's statistics using their user ID.
    public async Task<OrganizerStatistics?> GetByUserIdAsync(Guid userId)
    {
        return await DbSet
            .FirstOrDefaultAsync(x => x.UserId == userId);
    }
    // Adds a new organizer statistics record to the current database context.
    public async Task AddAsync(OrganizerStatistics statistics)
    {
        await AddEntityAsync(statistics);
    }
    // Marks the specified organizer statistics record as modified.
    public Task UpdateAsync(OrganizerStatistics statistics)
    {
        UpdateEntity(statistics);
        return Task.CompletedTask;
    }
}