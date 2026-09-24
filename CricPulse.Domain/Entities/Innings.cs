namespace CricPulse.Domain.Entities
{
    public class Innings
    {
        public int Id { get; set; }

        public int MatchId { get; set; }

        public int InningsNumber { get; set; }

        public string BattingTeam { get; set; } = string.Empty;

        public string BowlingTeam { get; set; } = string.Empty;

        public int StrikerMatchPlayerId { get; set; }

        public int NonStrikerMatchPlayerId { get; set; }

        public int? CurrentBowlerMatchPlayerId { get; set; }

        public int TotalRuns { get; set; }

        public int Wickets { get; set; }

        /// <summary>
        /// Number of legal deliveries bowled in this innings.
        /// Six legal deliveries = one completed over.
        /// </summary>
        public int LegalBalls { get; set; }

        public string Status { get; set; } = "NotStarted";

        /// <summary>
        /// True when the next delivery is a free hit.
        /// A free hit is awarded after a no-ball and remains active
        /// until a legal delivery is completed or another no-ball occurs.
        /// </summary>
        public bool IsFreeHit { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties

        public Match Match { get; set; } = null!;

        public MatchPlayer StrikerMatchPlayer { get; set; } = null!;

        public MatchPlayer NonStrikerMatchPlayer { get; set; } = null!;

        public MatchPlayer? CurrentBowlerMatchPlayer { get; set; }

        public ICollection<Ball> Balls { get; set; } = new List<Ball>();
    }
}