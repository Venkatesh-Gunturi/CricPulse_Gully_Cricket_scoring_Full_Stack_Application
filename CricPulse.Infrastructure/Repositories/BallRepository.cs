using CricPulse.Application.Interfaces.Repositories;
using CricPulse.Domain.Entities;
using CricPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CricPulse.Infrastructure.Repositories;

public class BallRepository : RepositoryBase<Ball>, IBallRepository
{
    public BallRepository(CricPulseDbContext context)
        : base(context)
    {
    }

    public async Task<Ball?> GetByIdAsync(Guid ballId)
    {
        return await DbSet
            .Include(x => x.Wicket)
            .FirstOrDefaultAsync(x => x.Id == ballId);
    }

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

    public Task DeleteAsync(Ball ball)
    {
        RemoveEntity(ball);
        return Task.CompletedTask;
    }
}