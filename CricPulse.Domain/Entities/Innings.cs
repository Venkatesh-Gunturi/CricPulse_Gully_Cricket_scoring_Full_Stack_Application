namespace CricPulse.Domain.Entities;

public class Innings
{
    public Guid Id { get; set; }

    public Guid MatchId { get; set; }

    public int InningsNumber { get; set; }

    public string BattingTeam { get; set; } = string.Empty;

    public string BowlingTeam { get; set; } = string.Empty;

    public int TotalRuns { get; set; }

    public int Wickets { get; set; }

    public int LegalBalls { get; set; }

    public int TotalOvers { get; set; }

    public int? TargetRuns { get; set; }

    public string Status { get; set; } = "Setup";

    public Guid? CurrentStrikerMatchPlayerId { get; set; }

    public Guid? CurrentNonStrikerMatchPlayerId { get; set; }

    public Guid? CurrentBowlerMatchPlayerId { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public Match Match { get; set; } = null!;

    public ICollection<Ball> Balls { get; set; } = new List<Ball>();
    public bool CompletionPending { get; set; }

    public bool IsFreeHit { get; set; }
    public bool UndoUsed { get; set; }
}