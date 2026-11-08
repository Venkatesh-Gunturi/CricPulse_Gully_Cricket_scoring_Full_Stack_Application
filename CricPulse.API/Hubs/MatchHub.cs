
using Microsoft.AspNetCore.SignalR;

namespace CricPulse.API.Hubs;

public class MatchHub : Hub
{
    public async Task JoinMatch(string matchId)
    {
        if (!Guid.TryParse(matchId, out var parsedMatchId))
            throw new HubException("Invalid match ID.");

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            parsedMatchId.ToString()
        );
    }

    public async Task LeaveMatch(string matchId)
    {
        if (!Guid.TryParse(matchId, out var parsedMatchId))
            throw new HubException("Invalid match ID.");

        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            parsedMatchId.ToString()
        );
    }
}
