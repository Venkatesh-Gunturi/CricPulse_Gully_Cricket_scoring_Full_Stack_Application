using CricPulse.Application.DTOs.Match;

namespace CricPulse.Application.Interfaces
{
    public interface IMatchScoringService
    {
        Task<bool> ScoreRunsAsync(int umpireId, int inningsId, int runs);
        Task<bool> ScoreWicketAsync (int umpireId, ScoreWicketDto dto);
        Task<bool> ScoreExtraAsync(int umpireId, ScoreExtraDto dto);
        Task<bool> UndoLastScoreAsync(int umpireId, UndoScoreDto dto);

    }
}