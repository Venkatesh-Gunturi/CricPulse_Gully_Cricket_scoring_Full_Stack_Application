using CricPulse.Application.Interfaces.Repositories;
using CricPulse.Domain.Entities;
using CricPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CricPulse.Infrastructure.Repositories;
// Handles database operations for Innings entities.
public class InningsRepository : RepositoryBase<Innings>, IInningsRepository
{
    public InningsRepository(CricPulseDbContext context)
        : base(context)
    {
    }
    // Retrieves an innings by its ID, including its match, balls, and associated wickets.
    public async Task<Innings?> GetByIdAsync(Guid inningsId)
    {
        return await DbSet
            .Include(x => x.Match)
            .Include(x => x.Balls)
                .ThenInclude(x => x.Wicket)
            .FirstOrDefaultAsync(x => x.Id == inningsId);
    }
    // Retrieves all innings for a match in innings-number order, including their balls and wickets.
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
    // Adds a new innings to the current database context.
    public async Task AddAsync(Innings innings)
    {
        await AddEntityAsync(innings);
    }
    // Marks the specified innings as modified in the current database context.
    public Task UpdateAsync(Innings innings)
    {
        UpdateEntity(innings);
        return Task.CompletedTask;
    }
}