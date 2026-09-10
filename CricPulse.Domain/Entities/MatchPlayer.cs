namespace CricPulse.Domain.Entities
{
    public class MatchPlayer
    {
        public int Id { get; set; }

        public int MatchId { get; set; }

        public int PlayerId { get; set; }

        public string Team { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public Match Match { get; set; } = null!;

        public Player Player { get; set; } = null!;

        public ICollection<Ball> BallsAsStriker { get; set; }
    = new List<Ball>();

        public ICollection<Ball> BallsAsNonStriker { get; set; }
            = new List<Ball>();

        public ICollection<Ball> BallsAsBowler { get; set; }
            = new List<Ball>();

        public ICollection<Wicket> Dismissals { get; set; }
    = new List<Wicket>();

        public ICollection<Wicket> Catches { get; set; }
            = new List<Wicket>();
    }
}