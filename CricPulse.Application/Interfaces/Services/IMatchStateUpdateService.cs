using CricPulse.Application.DTOs.Scoring;

namespace CricPulse.Application.Interfaces.Services;

public interface IMatchStateUpdateService
{
    Task<MatchStateDto> GetCurrentMatchStateAsync(Guid matchId);

    Task<MatchStateDto> GetAndBroadcastMatchStateAsync(Guid matchId);
}