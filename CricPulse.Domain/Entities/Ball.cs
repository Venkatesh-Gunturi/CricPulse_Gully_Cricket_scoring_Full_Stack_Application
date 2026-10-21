namespace CricPulse.Domain.Entities;

public class Ball
{
    public Guid Id { get; set; }

    public Guid InningsId { get; set; }

    public int OverNumber { get; set; }

    public int BallNumber { get; set; }

    public Guid StrikerMatchPlayerId { get; set; }

    public Guid NonStrikerMatchPlayerId { get; set; }

    public Guid BowlerMatchPlayerId { get; set; }

    public int Runs { get; set; }

    public int BatterRuns { get; set; }

    public bool IsLegalDelivery { get; set; }

    public string? ExtraType { get; set; }

    public int ExtraRuns { get; set; }

    public string? Notation { get; set; }

    public DateTime CreatedAt { get; set; }

    // Previous state used by one-ball undo.
    public Guid PreviousStrikerMatchPlayerId { get; set; }

    public Guid PreviousNonStrikerMatchPlayerId { get; set; }

    public Guid PreviousBowlerMatchPlayerId { get; set; }

    public int PreviousTotalRuns { get; set; }

    public int PreviousWickets { get; set; }

    public int PreviousLegalBalls { get; set; }

    public string PreviousInningsStatus { get; set; } = "Live";

    public bool PreviousCompletionPending { get; set; }

    public bool PreviousIsFreeHit { get; set; }

    public Innings Innings { get; set; } = null!;

    public MatchPlayer StrikerMatchPlayer { get; set; } = null!;

    public MatchPlayer NonStrikerMatchPlayer { get; set; } = null!;

    public MatchPlayer BowlerMatchPlayer { get; set; } = null!;

    public Wicket? Wicket { get; set; }
}