using CricPulse.Application.Interfaces.Repositories;
using CricPulse.Domain.Entities;
using CricPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CricPulse.Infrastructure.Repositories;

public class WicketRepository : RepositoryBase<Wicket>, IWicketRepository
{
    public WicketRepository(CricPulseDbContext context)
        : base(context)
    {
    }

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

    public async Task AddAsync(Wicket wicket)
    {
        await AddEntityAsync(wicket);
    }

    public Task DeleteAsync(Wicket wicket)
    {
        RemoveEntity(wicket);
        return Task.CompletedTask;
    }
}