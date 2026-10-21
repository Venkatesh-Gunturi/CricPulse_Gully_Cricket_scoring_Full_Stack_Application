using CricPulse.Application.Interfaces.Repositories;
using CricPulse.Domain.Entities;
using CricPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CricPulse.Infrastructure.Repositories;

public class PlayerStatisticsRepository : RepositoryBase<PlayerStatistics>, IPlayerStatisticsRepository
{
    public PlayerStatisticsRepository(CricPulseDbContext context)
        : base(context)
    {
    }

    public async Task<PlayerStatistics?> GetByUserIdAsync(Guid userId)
    {
        return await DbSet
            .FirstOrDefaultAsync(x => x.UserId == userId);
    }

    public async Task AddAsync(PlayerStatistics statistics)
    {
        await AddEntityAsync(statistics);
    }

    public Task UpdateAsync(PlayerStatistics statistics)
    {
        UpdateEntity(statistics);
        return Task.CompletedTask;
    }
}