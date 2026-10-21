using CricPulse.Domain.Entities;

namespace CricPulse.Application.Interfaces.Repositories;

public interface IWicketRepository
{
    Task<Wicket?> GetByBallIdAsync(Guid ballId);

    Task AddAsync(Wicket wicket);

    Task DeleteAsync(Wicket wicket);
}