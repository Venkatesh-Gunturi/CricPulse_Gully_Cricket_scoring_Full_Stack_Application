using CricPulse.Application.DTOs.Scoring;

namespace CricPulse.Application.Interfaces.Services;

public interface IScoringService
{
    Task<MatchStateDto> StartMatchAsync(StartMatchDto dto);

    Task<MatchStateDto> RecordTossAsync(
        Guid matchId,
        TossDto dto);

    Task<MatchStateDto> SetupInningsAsync(
        Guid matchId,
        SetupInningsDto dto);

    Task<MatchStateDto> ScoreRunsAsync(
        Guid inningsId,
        ScoreRunDto dto);

    Task<MatchStateDto> ScoreExtraAsync(
        Guid inningsId,
        ScoreExtraDto dto);

    Task<MatchStateDto> ScoreWicketAsync(
        Guid inningsId,
        ScoreWicketDto dto);

    Task<MatchStateDto> CompleteInningsAsync(
        Guid inningsId,
        CompleteInningsDto dto);

    Task<MatchStateDto> ChangeBowlerAsync(
        Guid inningsId,
        ChangeBowlerDto dto);

    Task<MatchStateDto> UndoLastBallAsync(
        Guid inningsId,
        Guid createdByUserId);

    Task<MatchStateDto> GetMatchStateAsync(Guid matchId);
}