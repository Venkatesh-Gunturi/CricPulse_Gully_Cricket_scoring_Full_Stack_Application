namespace CricPulse.Domain.Entities
{
    public class Wicket
    {
        public int Id { get; set; }

        public int BallId { get; set; }

        public int DismissedMatchPlayerId { get; set; }

        public string WicketType { get; set; } = string.Empty;

        public int? CaughtByMatchPlayerId { get; set; }

        public int RunsCompleted { get; set; }

        public Ball Ball { get; set; } = null!;

        public MatchPlayer DismissedMatchPlayer { get; set; } = null!;

        public MatchPlayer? CaughtByMatchPlayer { get; set; }
    }
}