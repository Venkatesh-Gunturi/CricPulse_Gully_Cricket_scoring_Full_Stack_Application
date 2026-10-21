using CricPulse.Domain.Entities;

namespace CricPulse.Application.Interfaces.Repositories;

public interface IInningsRepository
{
    Task<Innings?> GetByIdAsync(Guid inningsId);

    Task<IReadOnlyList<Innings>> GetByMatchIdAsync(Guid matchId);

    Task AddAsync(Innings innings);

    Task UpdateAsync(Innings innings);
}