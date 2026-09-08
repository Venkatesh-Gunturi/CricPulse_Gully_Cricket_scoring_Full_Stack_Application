using MatchEntity=CricPulse.Domain.Entities.Match;

namespace CricPulse.Application.Interfaces.Match
{
    public interface IMatchRepository
    {
        Task<MatchEntity> CreateAsync(MatchEntity match);

        Task<MatchEntity?> GetByIdAsync(int id);

        Task<List<MatchEntity>> GetAllAsync();

        Task<List<NearbyMatchResult>> GetNearbyMatchesAsync(
            double latitude,
            double longitude,
            double radiusInKm);

        Task<List<MatchEntity>> GetMatchesByStateAsync(string state);

        Task<MatchEntity?> UpdateAsync(MatchEntity match);

        Task<bool> CancelAsync(int matchId);

        Task<MatchEntity?> GetByIdForUpdateAsync(int matchId);
        Task<List<MatchEntity>> GetMatchesByUmpireAsync(int umpireId);
    }
}