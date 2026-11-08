
using CricPulse.Application.DTOs.Scoring;

namespace CricPulse.Application.Interfaces.Services;

public interface IMatchUpdateNotifier
{
    Task NotifyMatchStateUpdatedAsync(
        Guid matchId,
        MatchStateDto matchState);
}
