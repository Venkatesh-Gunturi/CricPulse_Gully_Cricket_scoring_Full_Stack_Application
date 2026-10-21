namespace CricPulse.Application.Interfaces.Services;

public interface IMatchCompletionService
{
    Task FinalizeCompletedMatchAsync(Guid matchId);
}