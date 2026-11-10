using CricPulse.Application.Interfaces.Repositories;
using CricPulse.Domain.Entities;
using CricPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CricPulse.Infrastructure.Repositories;
// Handles database operations for Ball entities.
public class BallRepository : RepositoryBase<Ball>, IBallRepository
{
    public BallRepository(CricPulseDbContext context) : base(context)
    {
    }
    // Retrieves a ball by ID along with its associated wicket.
    public async Task<Ball?> GetByIdAsync(Guid ballId)
    {
        return await DbSet
            .Include(x => x.Wicket)
            .FirstOrDefaultAsync(x => x.Id == ballId);
    }
    // Retrieves the most recently created ball in the specified innings,
    // including its associated wicket.
    public async Task<Ball?> GetLastBallAsync(Guid inningsId)
    {
        return await DbSet
            .Include(x => x.Wicket)
            .Where(x => x.InningsId == inningsId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(Ball ball)
    {
        await AddEntityAsync(ball);
    }
    // Marks the specified ball for deletion from the database.
    public Task DeleteAsync(Ball ball)
    {
        RemoveEntity(ball);
        return Task.CompletedTask;
    }
}