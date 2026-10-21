using CricPulse.Application.Interfaces.Services;

namespace CricPulse.API.Background;

public class MatchLifecycleWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MatchLifecycleWorker> _logger;

    public MatchLifecycleWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<MatchLifecycleWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope =
                    _scopeFactory.CreateScope();

                var lifecycleService =
                    scope.ServiceProvider
                        .GetRequiredService<IMatchLifecycleService>();

                await lifecycleService.ProcessLifecycleAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "CricPulse match lifecycle processing failed.");
            }

            await Task.Delay(
                TimeSpan.FromMinutes(5),
                stoppingToken);
        }
    }
}