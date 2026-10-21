using CricPulse.Application.Interfaces.Repositories;
using CricPulse.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CricPulse.Application.Services.Match;

public class MatchLifecycleService : IMatchLifecycleService
{
    private readonly IMatchRepository _matchRepository;
    private readonly IOrganizerStatisticsRepository _organizerStatisticsRepository;
    private readonly IMatchCompletionService _matchCompletionService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MatchLifecycleService> _logger;

    public MatchLifecycleService(
        IMatchRepository matchRepository,
        IOrganizerStatisticsRepository organizerStatisticsRepository,
        IMatchCompletionService matchCompletionService,
        IUnitOfWork unitOfWork, ILogger<MatchLifecycleService> logger
        )
    {
        _matchRepository = matchRepository;
        _organizerStatisticsRepository = organizerStatisticsRepository;
        _matchCompletionService = matchCompletionService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task ProcessLifecycleAsync()
    {
        await CancelExpiredUpcomingMatchesAsync();

        await FinalizeUnfinalizedFinishedMatchesAsync();

        await DeleteExpiredMatchesAsync();
    }

    private async Task CancelExpiredUpcomingMatchesAsync()
    {
        var matches =
            await _matchRepository.GetByStatusAsync("Upcoming");

        var now = DateTime.UtcNow;

        _logger.LogInformation(
            "Lifecycle worker found {Count} Upcoming matches at {NowUtc}.",
            matches.Count,
            now);

        foreach (var match in matches)
        {
            var cancellationDeadline = match.ScheduledAt.AddHours(24);

            if (cancellationDeadline > now)
                continue;

            _logger.LogInformation(
                "Auto-cancelling match {MatchId}. ScheduledAt: {ScheduledAt}, deadline: {Deadline}.",
                match.Id,
                match.ScheduledAt,
                cancellationDeadline);

            // Keep your existing cancellation and statistics logic here.
        }

        await _unitOfWork.SaveChangesAsync();
    }

    private async Task FinalizeUnfinalizedFinishedMatchesAsync()
    {
        var matches =
           await _matchRepository.GetByStatusAsync("Finished");

        foreach (var match in matches)
        {
            if (match.PlayerStatisticsFinalized)
                continue;

            await _matchCompletionService
                .FinalizeCompletedMatchAsync(match.Id);
        }
    }

    private async Task DeleteExpiredMatchesAsync()
    {
        var now = DateTime.UtcNow;
        var expiry = now.AddDays(-7);

        var finishedMatches = await _matchRepository.GetByStatusAsync("Finished");

        foreach (var match in finishedMatches)
        {
            if (!match.PlayerStatisticsFinalized)
                continue;

            if (!match.FinishedAt.HasValue)
                continue;

            if (match.FinishedAt.Value > expiry)
                continue;

            await _matchRepository.DeleteAsync(match);
        }

        var cancelledMatches =
            await _matchRepository.GetByStatusAsync("Cancelled");

        foreach (var match in cancelledMatches)
        {
            if (!match.CancelledAt.HasValue)
                continue;

            if (match.CancelledAt.Value > expiry)
                continue;

            await _matchRepository.DeleteAsync(match);
        }

        await _unitOfWork.SaveChangesAsync();
    }
}