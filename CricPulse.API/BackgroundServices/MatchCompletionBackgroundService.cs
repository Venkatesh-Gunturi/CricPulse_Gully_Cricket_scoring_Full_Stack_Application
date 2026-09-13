using CricPulse.Application.Interfaces.Match;
using CricPulse.Domain.Enums;

namespace CricPulse.Api.Services
{
    public class MatchCompletionBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MatchCompletionBackgroundService> _logger;

        public MatchCompletionBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<MatchCompletionBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        // Purpose:
        // Periodically complete matches whose 10-second completion grace period
        // has expired without requiring any action from the umpire's browser.
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
                            .GetPendingCompletionMatchesAsync();

                    foreach (var match in matches)
                    {
                        match.Status = MatchStatus.Completed;
                        match.CompletionDeadline = null;
                        match.UpdatedAt = DateTime.UtcNow;
                    }

                    if (matches.Count > 0)
                    {
                        await scoringRepository.SaveChangesAsync();

                        _logger.LogInformation(
                            "Automatically completed {Count} match(es) after the completion grace period.",
                            matches.Count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error while processing pending match completions.");
                }

                await Task.Delay(
                    TimeSpan.FromSeconds(1),
                    stoppingToken);
            }
        }
    }
}