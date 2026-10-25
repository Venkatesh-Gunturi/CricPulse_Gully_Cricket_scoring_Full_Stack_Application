namespace CricPulse.Application.DTOs.Scoring;

public class MatchStateDto
{
    public Guid MatchId { get; set; }

    public string Status { get; set; } = string.Empty;

    public string TeamAName { get; set; } = string.Empty;

    public string TeamBName { get; set; } = string.Empty;

    public int Overs { get; set; }

    public int PlayersPerTeam { get; set; }

    public string? TossWinnerTeam { get; set; }

    public string? TossDecision { get; set; }

    public bool TossAgreed { get; set; }

    public string? WinnerTeam { get; set; }

    public string? ResultText { get; set; }

    public List<InningsStateDto> Innings { get; set; } = new();
}

public class InningsStateDto
{
    public Guid InningsId { get; set; }

    public int InningsNumber { get; set; }

    public string BattingTeam { get; set; } = string.Empty;

    public string BowlingTeam { get; set; } = string.Empty;

    public int TotalRuns { get; set; }

    public int Wickets { get; set; }

    public int LegalBalls { get; set; }

    public int TotalOvers { get; set; }

    public int? TargetRuns { get; set; }

    public int? RunsRequired { get; set; }

    public int? BallsRemaining { get; set; }

    public decimal? CurrentRunRate { get; set; }

    public decimal? RequiredRunRate { get; set; }

    public string Status { get; set; } = string.Empty;

    public bool CompletionPending { get; set; }

    public bool IsFreeHit { get; set; }

    public bool UndoUsed { get; set; }
    public List<MatchPlayerOptionDto> BattingPlayers { get; set; } = new();

    public List<MatchPlayerOptionDto> FieldingPlayers { get; set; } = new();
    public List<YetToBatPlayerDto> YetToBatPlayers { get; set; } = new();

    public List<BatterRecordDto> BattingRecords { get; set; } = new();

    public List<BowlerRecordDto> BowlingRecords { get; set; } = new();


    public Guid? StrikerMatchPlayerId { get; set; }

    public Guid? NonStrikerMatchPlayerId { get; set; }

    public Guid? BowlerMatchPlayerId { get; set; }
    public string? StrikerName { get; set; }
    public string? NonStrikerName { get; set; }
    public string? BowlerName { get; set; }

    public int StrikerRuns { get; set; }
    public int StrikerBalls { get; set; }
    public int StrikerFours { get; set; }
    public int StrikerSixes { get; set; }

    public int NonStrikerRuns { get; set; }
    public int NonStrikerBalls { get; set; }
    public int NonStrikerFours { get; set; }
    public int NonStrikerSixes { get; set; }

    public decimal BowlerOvers { get; set; }
    public int BowlerRunsConceded { get; set; }
    public int BowlerWickets { get; set; }

    public List<string> YetToBat { get; set; } = new();
    public List<string> CurrentOver { get; set; } = new();
}