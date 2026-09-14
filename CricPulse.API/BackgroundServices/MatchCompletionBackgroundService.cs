using CricPulse.Application.Interfaces;
using CricPulse.Application.Interfaces.Match;

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
        // Automatically finalize matches whose completion grace period has expired
        // by using the same completion workflow as manual umpire confirmation.
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

                    var scoringService =
                        scope.ServiceProvider
                            .GetRequiredService<IMatchScoringService>();

                    var matches =
                        await scoringRepository
                            .GetPendingCompletionMatchesAsync();

                    foreach (var match in matches)
                    {
                        var completed =
                            await scoringService.CompleteMatchAsync(
                                match.UmpireId,
                                match.Id);

                        if (completed)
                        {
                            _logger.LogInformation(
                                "Automatically completed match {MatchId} after the completion grace period.",
                                match.Id);
                        }
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