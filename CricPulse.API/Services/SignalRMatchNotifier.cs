
using CricPulse.API.Hubs;
using CricPulse.Application.DTOs.Scoring;
using CricPulse.Application.Interfaces.Services;
using Microsoft.AspNetCore.SignalR;

namespace CricPulse.API.Services;

public class SignalRMatchNotifier : IMatchUpdateNotifier
{
    private readonly IHubContext<MatchHub> _hubContext;
    private readonly ILogger<SignalRMatchNotifier> _logger;

    public SignalRMatchNotifier(
        IHubContext<MatchHub> hubContext,
        ILogger<SignalRMatchNotifier> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyMatchStateUpdatedAsync(
        Guid matchId,
        MatchStateDto matchState)
    {
        try
        {
            await _hubContext.Clients
                .Group(matchId.ToString())
                .SendAsync("MatchStateUpdated", matchState);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to broadcast match state update for match {MatchId}.",
                matchId);
        }
    }
}
