
using CricPulse.Application.Interfaces.Repositories;
using CricPulse.Application.Interfaces.Services;
using CricPulse.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CricPulse.Application.Services.Match;

public class MatchLifecycleService : IMatchLifecycleService
{
    private readonly IMatchRepository _matchRepository;
    private readonly IOrganizerStatisticsRepository _organizerStatisticsRepository;
    private readonly IMatchCompletionService _matchCompletionService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<MatchLifecycleService> _logger;

    public MatchLifecycleService(
        IMatchRepository matchRepository,
        IOrganizerStatisticsRepository organizerStatisticsRepository,
        IMatchCompletionService matchCompletionService,
        IUnitOfWork unitOfWork,
        ITransactionManager transactionManager,
        ILogger<MatchLifecycleService> logger)
    {
        _matchRepository = matchRepository;
        _organizerStatisticsRepository = organizerStatisticsRepository;
        _matchCompletionService = matchCompletionService;
        _unitOfWork = unitOfWork;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task ProcessLifecycleAsync()
    {
        await CancelUnstartedUpcomingMatchesAsync();
        await FinalizeUnfinalizedFinishedMatchesAsync();
    }

    private async Task CancelUnstartedUpcomingMatchesAsync()
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
            // Only cancel matches that are still Upcoming
            // and have not started.
            if (!string.Equals(
                    match.Status,
                    "Upcoming",
                    StringComparison.OrdinalIgnoreCase) ||
                match.ActualStartedAt.HasValue)
            {
                continue;
            }

            // Allow the umpire 24 hours after the scheduled time.
            var cancellationDeadline = match.ScheduledAt.AddHours(24);

            if (cancellationDeadline > now)
                continue;

            var organizerStatistics =
                await _organizerStatisticsRepository
                    .GetByUserIdAsync(match.CreatedByUserId);

            var createOrganizerStatistics =
                organizerStatistics is null;

            organizerStatistics ??= new OrganizerStatistics
            {
                UserId = match.CreatedByUserId
            };

            organizerStatistics.MatchesCancelled++;

            match.Status = "Cancelled";
            match.CancellationReasonType = "Automatic";
            match.CancellationReason = "Umpire unavailable";
            match.CancelledAt = now;
            match.UpdatedAt = now;

            await _transactionManager.BeginTransactionAsync();

            try
            {
                await _matchRepository.UpdateAsync(match);

                if (createOrganizerStatistics)
                {
                    await _organizerStatisticsRepository
                        .AddAsync(organizerStatistics);
                }
                else
                {
                    await _organizerStatisticsRepository
                        .UpdateAsync(organizerStatistics);
                }

                await _unitOfWork.SaveChangesAsync();

                await _transactionManager.CommitTransactionAsync();

                _logger.LogInformation(
                    "Match {MatchId} automatically cancelled. " +
                    "Reason: Umpire unavailable. Organizer: {OrganizerId}.",
                    match.Id,
                    match.CreatedByUserId);
            }
            catch (Exception ex)
            {
                await _transactionManager.RollbackTransactionAsync();

                _logger.LogError(
                    ex,
                    "Failed to automatically cancel match {MatchId}.",
                    match.Id);

                throw;
            }
        }
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

        var finishedMatches =
            await _matchRepository.GetByStatusAsync("Finished");

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
