// Purpose:
// Periodically cancel scheduled matches whose 24-hour start window has expired.
using CricPulse.Application.Interfaces.Match;
using CricPulse.Domain.Enums;

public class MatchCancellationService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public MatchCancellationService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    // Purpose:
    // Automatically cancel scheduled matches that were not started within their allowed time window.
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();

            var repository = scope.ServiceProvider
                .GetRequiredService<IScoringRepository>();

            var expiredMatches = await repository
                .GetExpiredScheduledMatchesAsync();

            foreach (var match in expiredMatches)
            {
                match.Status = MatchStatus.Cancelled;
                match.CancellationReason = "Umpire unavailable";
                match.UpdatedAt = DateTime.UtcNow;
            }

            if (expiredMatches.Count > 0)
            {
                await repository.SaveChangesAsync();
            }

            await Task.Delay(
                TimeSpan.FromMinutes(5),
                stoppingToken);
        }
    }
}