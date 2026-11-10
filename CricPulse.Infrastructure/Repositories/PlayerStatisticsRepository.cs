using CricPulse.Application.Interfaces.Repositories;
using CricPulse.Domain.Entities;
using CricPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CricPulse.Infrastructure.Repositories;
// Handles database operations for PlayerStatistics entities.
public class PlayerStatisticsRepository : RepositoryBase<PlayerStatistics>, IPlayerStatisticsRepository
{
    public PlayerStatisticsRepository(CricPulseDbContext context) : base(context)
    {
    }
    // Retrieves a player's statistics using their user ID.
    public async Task<PlayerStatistics?> GetByUserIdAsync(Guid userId)
    {
        return await DbSet
            .FirstOrDefaultAsync(x => x.UserId == userId);
    }

    // Adds a new player statistics record to the current database context.
    public async Task AddAsync(PlayerStatistics statistics)
    {
        await AddEntityAsync(statistics);
    }

    // Marks the specified player statistics record as modified.
    public Task UpdateAsync(PlayerStatistics statistics)
    {
        UpdateEntity(statistics);
        return Task.CompletedTask;
    }
}