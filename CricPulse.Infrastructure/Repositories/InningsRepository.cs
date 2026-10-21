using CricPulse.Application.Interfaces.Repositories;
using CricPulse.Domain.Entities;
using CricPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CricPulse.Infrastructure.Repositories;

public class InningsRepository : RepositoryBase<Innings>, IInningsRepository
{
    public InningsRepository(CricPulseDbContext context)
        : base(context)
    {
    }

    public async Task<Innings?> GetByIdAsync(Guid inningsId)
    {
        return await DbSet
            .Include(x => x.Match)
            .Include(x => x.Balls)
                .ThenInclude(x => x.Wicket)
            .FirstOrDefaultAsync(x => x.Id == inningsId);
    }

    public async Task<IReadOnlyList<Innings>> GetByMatchIdAsync(Guid matchId)
    {
        return await DbSet
            .AsNoTracking()
            .Include(x => x.Balls)
                .ThenInclude(x => x.Wicket)
            .Where(x => x.MatchId == matchId)
            .OrderBy(x => x.InningsNumber)
            .ToListAsync();
    }

    public async Task AddAsync(Innings innings)
    {
        await AddEntityAsync(innings);
    }

    public Task UpdateAsync(Innings innings)
    {
        UpdateEntity(innings);
        return Task.CompletedTask;
    }
}