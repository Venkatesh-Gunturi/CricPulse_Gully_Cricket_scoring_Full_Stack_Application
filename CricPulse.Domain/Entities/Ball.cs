namespace CricPulse.Domain.Entities
{
    public class Ball
    {
        public int Id { get; set; }

        public int InningsId { get; set; }

        public int OverNumber { get; set; }

        public int BallNumber { get; set; }

        public int StrikerMatchPlayerId { get; set; }

        public int NonStrikerMatchPlayerId { get; set; }

        public int BowlerMatchPlayerId { get; set; }

        public int Runs { get; set; }

        public bool IsLegalDelivery { get; set; }

        public string? ExtraType { get; set; }

        public int ExtraRuns { get; set; }

        public DateTime CreatedAt { get; set; }

        public Innings Innings { get; set; } = null!;

        public MatchPlayer StrikerMatchPlayer { get; set; } = null!;

        public MatchPlayer NonStrikerMatchPlayer { get; set; } = null!;

        public MatchPlayer BowlerMatchPlayer { get; set; } = null!;

        public Wicket? Wicket { get; set; }
    }
}