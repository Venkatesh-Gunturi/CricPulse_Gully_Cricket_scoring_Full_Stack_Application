using CricPulse.Application.Interfaces.Repositories;
using CricPulse.Domain.Entities;
using CricPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CricPulse.Infrastructure.Repositories;
// Handles database operations for Wicket entities.
public class WicketRepository : RepositoryBase<Wicket>, IWicketRepository
{
    public WicketRepository(CricPulseDbContext context) : base(context)
    {
    }
    // Retrieves a wicket by ball ID, including the dismissed player
    // and any players involved in the dismissal.
    public async Task<Wicket?> GetByBallIdAsync(Guid ballId)
    {
        return await DbSet
            .Include(x => x.DismissedMatchPlayer)
                .ThenInclude(x => x.User)
            .Include(x => x.CaughtByMatchPlayer)
                .ThenInclude(x => x!.User)
            .Include(x => x.RunOutByMatchPlayer)
                .ThenInclude(x => x!.User)
            .Include(x => x.StumpedByMatchPlayer)
                .ThenInclude(x => x!.User)
            .FirstOrDefaultAsync(x => x.BallId == ballId);
    }

    // Adds a new wicket to the current database context.
    public async Task AddAsync(Wicket wicket)
    {
        await AddEntityAsync(wicket);
    }

    // Marks the specified wicket for deletion from the database.
    public Task DeleteAsync(Wicket wicket)
    {
        RemoveEntity(wicket);
        return Task.CompletedTask;
    }
}