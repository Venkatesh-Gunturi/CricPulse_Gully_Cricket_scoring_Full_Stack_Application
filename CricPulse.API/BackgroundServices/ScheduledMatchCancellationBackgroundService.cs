using CricPulse.Application.Interfaces.Match;
using CricPulse.Domain.Enums;

namespace CricPulse.Api.Services
{
    public class ScheduledMatchCancellationBackgroundService
        : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ScheduledMatchCancellationBackgroundService> _logger;

        public ScheduledMatchCancellationBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<ScheduledMatchCancellationBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        // Purpose:
        // Automatically cancel scheduled matches that were not started within
        // 24 hours of their latest scheduled date and time.
        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var scoringRepository =
                        scope.ServiceProvider
                            .GetRequiredService<IScoringRepository>();

                    var matches =
                        await scoringRepository
                            .GetExpiredScheduledMatchesAsync();

                    foreach (var match in matches)
                    {
                        // Only scheduled matches can be automatically cancelled.
                        if (match.Status != MatchStatus.Scheduled)
                        {
                            continue;
                        }

                        match.Status = MatchStatus.Cancelled;
                        match.CancellationReason =
                            "Umpire unavailable";
                        match.UpdatedAt = DateTime.UtcNow;
                    }

                    if (matches.Count > 0)
                    {
                        await scoringRepository.SaveChangesAsync();

                        _logger.LogInformation(
                            "Automatically cancelled {Count} scheduled match(es) because they were not started within 24 hours.",
                            matches.Count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error while processing expired scheduled matches.");
                }

                await Task.Delay(
                TimeSpan.FromMinutes(1),
                 stoppingToken);
            }
        }
    }
}