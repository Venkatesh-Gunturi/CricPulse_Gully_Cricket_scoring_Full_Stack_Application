using CricPulse.Domain.Entities;

namespace CricPulse.Application.Interfaces.Repositories;

public interface IMatchScoringRepository
{
    Task<Innings?> GetInningsForScoringAsync(Guid inningsId);

    Task<Ball?> GetLastBallAsync(Guid inningsId);

    Task AddBallAsync(Ball ball);

    Task AddWicketAsync(Wicket wicket);

    Task DeleteBallAsync(Ball ball);

    Task UpdateInningsAsync(Innings innings);

    Task SaveChangesAsync();
}