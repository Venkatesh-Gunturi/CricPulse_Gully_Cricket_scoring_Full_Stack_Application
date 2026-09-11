using CricPulse.Domain.Entities;
using MatchEntity = CricPulse.Domain.Entities.Match;
namespace CricPulse.Application.Interfaces.Match
{
    public interface IScoringRepository
    {
        Task<Innings?> GetInningsForScoringAsync(int inningsId);
        Task AddBallAsync(Ball ball);
        Task AddWicketAsync(Wicket wicket);
        Task SaveChangesAsync();

        // Purpose:
        // Remove the specified scoring action and its associated wicket, if any,
        // so the latest scoring action can be safely undone.
        Task RemoveBallAsync(Ball ball);

        Task<MatchEntity?> GetMatchForTossAsync(int matchId);
        Task<List<MatchEntity>> GetExpiredScheduledMatchesAsync();

        Task<MatchEntity?> GetMatchForInningsAsync(int matchId);
    }
}