namespace CricPulse.Application.Interfaces.Player
{
    public interface IPlayerStatisticsService
    {
        Task CalculateAndSaveMatchStatisticsAsync(int matchId);
    }
}