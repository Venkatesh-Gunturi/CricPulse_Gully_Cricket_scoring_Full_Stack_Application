namespace CricPulse.Application.DTOs.Match
{
    public class LiveMatchResponseDto
    {
        // ================================================================
        // MATCH INFO
        // ================================================================

        public int MatchId { get; set; }

        public string Team1Name { get; set; } = string.Empty;

        public string Team1Logo { get; set; } = string.Empty;

        public string Team2Name { get; set; } = string.Empty;

        public string Team2Logo { get; set; } = string.Empty;

        public int PlayersPerTeam { get; set; }

        public int Overs { get; set; }

        public DateTime MatchDate { get; set; }

        public TimeSpan MatchTime { get; set; }

        public string VenueName { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string State { get; set; } = string.Empty;

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string? LiveStreamUrl { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime? StartedAt { get; set; }

        public DateTime CreatedAt { get; set; }

        // ================================================================
        // TOSS
        // ================================================================

        public string? TossWinnerTeam { get; set; }

        public string? TossDecision { get; set; }

        public string? BattingFirstTeam { get; set; }

        // ================================================================
        // MATCH RESULT
        // ================================================================

        public string? Result { get; set; }

        public DateTime? CompletionDeadline { get; set; }

        // ================================================================
        // PLAYERS
        // ================================================================

        public List<LiveMatchPlayerDto> Players { get; set; } = new();

        // ================================================================
        // CURRENT INNINGS
        // ================================================================

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

        public string? InningsStatus { get; set; }

        /// <summary>
        /// True when the next delivery is a free hit.
        /// </summary>
        public bool IsFreeHit { get; set; }

        // ================================================================
        // FIRST INNINGS
        // ================================================================

        public int? FirstInningsTotalRuns { get; set; }

        public int? FirstInningsWickets { get; set; }

        public int? FirstInningsLegalBalls { get; set; }

        // ================================================================
        // CURRENT OVER
        // ================================================================

        public int CurrentOverNumber { get; set; }

        public List<LiveBallDto> CurrentOverBalls { get; set; } = new();

        // ================================================================
        // COMPLETE BALL HISTORY
        // ================================================================

        public List<LiveBallDto> Balls { get; set; } = new();
    }


    public class LiveMatchPlayerDto
    {
        public int MatchPlayerId { get; set; }

        public int PlayerId { get; set; }

        public string PlayerName { get; set; } = string.Empty;

        public string MobileNumber { get; set; } = string.Empty;

        public string Team { get; set; } = string.Empty;

        public bool IsCaptain { get; set; }

        public bool IsDismissed { get; set; }
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

        public int BatterRuns { get; set; }

        public bool IsLegalDelivery { get; set; }

        public string? ExtraType { get; set; }

        public int ExtraRuns { get; set; }

        public string Notation { get; set; } = string.Empty;

        public string? WicketType { get; set; }

        public int? DismissedMatchPlayerId { get; set; }

        public int? CaughtByMatchPlayerId { get; set; }

        public int? StumpedByMatchPlayerId { get; set; }

        public int RunsCompleted { get; set; }

        public bool DismissedPlayerWasStriker { get; set; }

        public bool DidBattersCross { get; set; }

        public bool FreeHitAfterDelivery { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}