using CricPulse.Application.Interfaces.Repositories;
using CricPulse.Domain.Entities;
using CricPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CricPulse.Infrastructure.Repositories;

public class OrganizerStatisticsRepository : RepositoryBase<OrganizerStatistics>, IOrganizerStatisticsRepository
{
    public OrganizerStatisticsRepository(CricPulseDbContext context)
        : base(context)
    {
    }

    public async Task<OrganizerStatistics?> GetByUserIdAsync(Guid userId)
    {
        return await DbSet
            .FirstOrDefaultAsync(x => x.UserId == userId);
    }

    public async Task AddAsync(OrganizerStatistics statistics)
    {
        await AddEntityAsync(statistics);
    }

    public Task UpdateAsync(OrganizerStatistics statistics)
    {
        UpdateEntity(statistics);
        return Task.CompletedTask;
    }
}