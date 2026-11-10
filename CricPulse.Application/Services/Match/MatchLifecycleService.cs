
using CricPulse.Application.Interfaces.Repositories;
using CricPulse.Application.Interfaces.Services;
using CricPulse.Application.Services;
using CricPulse.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CricPulse.Application.Services.Match;
// Manages automatic match cancellation, completion finalization,
// and deletion based on the match lifecycle and retention rules.
public class MatchLifecycleService : IMatchLifecycleService
{
    private readonly IMatchRepository _matchRepository;
    private readonly IOrganizerStatisticsRepository _organizerStatisticsRepository;
    private readonly IMatchCompletionService _matchCompletionService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<MatchLifecycleService> _logger;
    private readonly IMatchStateUpdateService _matchStateUpdateService;

    public MatchLifecycleService(
        IMatchRepository matchRepository,
        IOrganizerStatisticsRepository organizerStatisticsRepository,
        IMatchCompletionService matchCompletionService,
        IUnitOfWork unitOfWork,
        ITransactionManager transactionManager,
        ILogger<MatchLifecycleService> logger,
        IMatchStateUpdateService matchStateUpdateService)
    {
        _matchRepository = matchRepository;
        _organizerStatisticsRepository = organizerStatisticsRepository;
        _matchCompletionService = matchCompletionService;
        _unitOfWork = unitOfWork;
        _transactionManager = transactionManager;
        _logger = logger;
        _matchStateUpdateService = matchStateUpdateService;
    }

    // Runs all scheduled match lifecycle operations in sequence:
    // cancels unstarted matches, finalizes finished matches,
    // and removes matches that have exceeded the retention period.
    public async Task ProcessLifecycleAsync()
    {
        await CancelUnstartedUpcomingMatchesAsync();
        await FinalizeUnfinalizedFinishedMatchesAsync();
        await DeleteExpiredMatchesAsync();
    }

    // Automatically cancels Upcoming matches that were never started
    // within 24 hours of their scheduled start time.
    // Updates the organizer's cancellation statistics and broadcasts
    // the updated match state after the transaction is committed.
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

            // Calculate the cancellation deadline using the scheduled match time.
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
            // Save the match cancellation and organizer statistics together
            // so both changes succeed or roll back as one operation.
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
                // Notify connected clients only after the database transaction succeeds.
                await _matchStateUpdateService.GetAndBroadcastMatchStateAsync(match.Id);


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

    // Ensures player statistics are finalized for every Finished match
    // before the match becomes eligible for automatic deletion.
    private async Task FinalizeUnfinalizedFinishedMatchesAsync()
    {
        var matches =
            await _matchRepository.GetByStatusAsync("Finished");

        foreach (var match in matches)
        {
            // Skip matches whose player statistics have already been finalized.
            if (match.PlayerStatisticsFinalized)
                continue;

            await _matchCompletionService
                .FinalizeCompletedMatchAsync(match.Id);
        }
    }

    // Deletes Finished and Cancelled matches once they are at least
    // seven days old, based on their actual completion or cancellation time.
    // Finished matches are deleted only after player statistics are finalized.
    private async Task DeleteExpiredMatchesAsync()
    {
        var now = DateTime.UtcNow;
        var expiry = now.AddDays(-7);

        _logger.LogInformation(
            "Starting expired match deletion. Current UTC: {NowUtc}, Expiry cutoff: {ExpiryUtc}",
            now,
            expiry);

        // Delete Finished matches after verifying that statistics are finalized
        // and the match has been finished for at least seven days.

        var finishedMatches =
            await _matchRepository.GetByStatusAsync("Finished");

        _logger.LogInformation(
            "Found {Count} Finished matches.",
            finishedMatches.Count);

        foreach (var match in finishedMatches)
        {
            if (!match.PlayerStatisticsFinalized)
            {
                _logger.LogWarning(
                    "Skipping Finished match {MatchId}: Player statistics are not finalized.",
                    match.Id);

                continue;
            }

            if (!match.FinishedAt.HasValue)
            {
                _logger.LogWarning(
                    "Skipping Finished match {MatchId}: FinishedAt is null.",
                    match.Id);

                continue;
            }

            if (match.FinishedAt.Value > expiry)
            {
                _logger.LogInformation(
                    "Skipping Finished match {MatchId}: FinishedAt {FinishedAt} is not yet 7 days old.",
                    match.Id,
                    match.FinishedAt.Value);

                continue;
            }

            _logger.LogInformation(
                "Deleting Finished match {MatchId}. FinishedAt: {FinishedAt}",
                match.Id,
                match.FinishedAt.Value);

            await _matchRepository.DeleteAsync(match);
        }

        // Delete Cancelled matches that have remained cancelled for at least seven days.

        var cancelledMatches =
            await _matchRepository.GetByStatusAsync("Cancelled");

        _logger.LogInformation(
            "Found {Count} Cancelled matches.",
            cancelledMatches.Count);

        foreach (var match in cancelledMatches)
        {
            if (!match.CancelledAt.HasValue)
            {
                _logger.LogWarning(
                    "Skipping Cancelled match {MatchId}: CancelledAt is null.",
                    match.Id);

                continue;
            }

            if (match.CancelledAt.Value > expiry)
            {
                _logger.LogInformation(
                    "Skipping Cancelled match {MatchId}: CancelledAt {CancelledAt} is not yet 7 days old.",
                    match.Id,
                    match.CancelledAt.Value);

                continue;
            }

            _logger.LogInformation(
                "Deleting Cancelled match {MatchId}. CancelledAt: {CancelledAt}",
                match.Id,
                match.CancelledAt.Value);

            await _matchRepository.DeleteAsync(match);
        }

        // Persist all eligible match deletions in a single save operation.

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Expired match deletion processing completed successfully.");
    }
}
