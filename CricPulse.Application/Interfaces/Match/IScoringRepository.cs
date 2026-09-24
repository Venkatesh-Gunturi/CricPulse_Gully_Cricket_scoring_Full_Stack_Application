using CricPulse.Domain.Entities;
using MatchEntity = CricPulse.Domain.Entities.Match;
namespace CricPulse.Application.Interfaces.Match
{
    public interface IScoringRepository
    {
        // ================================================================
        // INNINGS / SCORING
        // ================================================================

        Task<Innings?> GetInningsForScoringAsync(int inningsId);

        Task AddBallAsync(Ball ball);

        Task AddWicketAsync(Wicket wicket);

        Task RemoveBallAsync(Ball ball);

        Task SaveChangesAsync();

        // ================================================================
        // MATCH / TOSS
        // ================================================================

        Task<MatchEntity?> GetMatchForTossAsync(int matchId);

        Task<MatchEntity?> GetMatchForInningsAsync(int matchId);

        // ================================================================
        // MATCH LIFECYCLE
        // ================================================================

        Task<List<MatchEntity>> GetExpiredScheduledMatchesAsync();

        Task<List<MatchEntity>> GetPendingCompletionMatchesAsync();

        // ================================================================
        // STATISTICS
        // ================================================================

        Task<MatchEntity?> GetMatchForStatisticsAsync(int matchId);

        // ================================================================
        // LEGACY / COMPATIBILITY
        // ================================================================

        Task DeleteCompletedMatchAsync(int matchId);
    }
}