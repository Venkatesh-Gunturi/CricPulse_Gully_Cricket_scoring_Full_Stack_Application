using CricPulse.Domain.Entities;

namespace CricPulse.Application.Interfaces.Repositories;

public interface IBallRepository
{
    Task<Ball?> GetByIdAsync(Guid ballId);

    Task<Ball?> GetLastBallAsync(Guid inningsId);

    Task AddAsync(Ball ball);

    Task DeleteAsync(Ball ball);
}