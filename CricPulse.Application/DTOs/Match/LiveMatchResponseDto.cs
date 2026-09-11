// Purpose:
// Provide the live match state required by the scoring and live-view screens.
namespace CricPulse.Application.DTOs.Match
{
    public class LiveMatchResponseDto
    {
        public int MatchId { get; set; }

        public string Team1Name { get; set; } = string.Empty;
        public string Team2Name { get; set; } = string.Empty;
        public string Team1Logo { get; set; } = string.Empty;
        public string Team2Logo { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public string? TossWinnerTeam { get; set; }
        public string? TossDecision { get; set; }
        public string? BattingFirstTeam { get; set; }

        public int? InningsId { get; set; }
        public int? InningsNumber { get; set; }

        public string? BattingTeam { get; set; }
        public string? BowlingTeam { get; set; }

        public int? StrikerMatchPlayerId { get; set; }
        public int? NonStrikerMatchPlayerId { get; set; }
        public int? CurrentBowlerMatchPlayerId { get; set; }

        public int? TotalRuns { get; set; }
        public int? Wickets { get; set; }
        public int? LegalBalls { get; set; }

        public List<LiveBallDto> Balls { get; set; } = new();
    }

    public class LiveBallDto
    {
        public int Id { get; set; }
        public int OverNumber { get; set; }
        public int BallNumber { get; set; }
        public int StrikerMatchPlayerId { get; set; }
        public int NonStrikerMatchPlayerId { get; set; }
        public int BowlerMatchPlayerId { get; set; }
        public int Runs { get; set; }
        public bool IsLegalDelivery { get; set; }
        public string? ExtraType { get; set; }
        public int ExtraRuns { get; set; }
        public string? WicketType { get; set; }
        public int? DismissedMatchPlayerId { get; set; }

       
    }
}