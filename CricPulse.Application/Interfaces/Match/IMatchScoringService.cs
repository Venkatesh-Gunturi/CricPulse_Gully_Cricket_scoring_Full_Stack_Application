using CricPulse.Application.DTOs.Match;

namespace CricPulse.Application.Interfaces.Match
{
    public interface IMatchScoringService
    {
        // ================================================================
        // SCORING
        // ================================================================

        Task<bool> ScoreRunsAsync(
            int umpireId,
            int inningsId,
            int runs);

        Task<bool> ScoreWicketAsync(
            int umpireId,
            ScoreWicketDto dto);

        Task<bool> ScoreExtraAsync(
            int umpireId,
            ScoreExtraDto dto);

        // ================================================================
        // BOWLER
        // ================================================================

        Task<bool> ChangeBowlerAsync(
            int umpireId,
            int inningsId,
            int newBowlerMatchPlayerId);

        // ================================================================
        // UNDO
        // ================================================================

        Task<bool> UndoLastScoreAsync(
            int umpireId,
            UndoScoreDto dto);

        // ================================================================
        // TOSS / INNINGS
        // ================================================================

        Task<bool> RecordTossAsync(
            int umpireId,
            RecordTossDto dto);

        Task<bool> StartInningsAsync(
            int umpireId,
            StartInningsDto dto);

        // ================================================================
        // MATCH COMPLETION
        // ================================================================

        Task<bool> CompleteMatchAsync(
            int umpireId,
            int matchId);
    }
}

