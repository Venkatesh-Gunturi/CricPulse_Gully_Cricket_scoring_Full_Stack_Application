

namespace CricPulse.Domain.Entities
{
    public class Wicket
    {
        public int Id { get; set; }

        public int BallId { get; set; }

        public int DismissedMatchPlayerId { get; set; }

        public string WicketType { get; set; } = string.Empty;

        /// <summary>
        /// Used for Caught.
        /// Any player from the fielding team can be selected
        /// because CricPulse supports gully-cricket fielding.
        /// </summary>
        public int? CaughtByMatchPlayerId { get; set; }

        /// <summary>
        /// Used for Stumped.
        /// Any player from the fielding team can be selected
        /// because CricPulse supports gully-cricket fielding.
        /// </summary>
        public int? StumpedByMatchPlayerId { get; set; }

        /// <summary>
        /// Number of completed runs before the dismissal.
        /// For Run Out this comes from the run-out flow.
        /// </summary>
        public int RunsCompleted { get; set; }

        /// <summary>
        /// True when the dismissed batter was the striker
        /// when the delivery started.
        /// </summary>
        public bool DismissedPlayerWasStriker { get; set; }

        /// <summary>
        /// Relevant to Run Out.
        /// Indicates whether the batters had crossed before
        /// the wicket was broken.
        /// </summary>
        public bool DidBattersCross { get; set; }

        // Navigation properties

        public Ball Ball { get; set; } = null!;

        public MatchPlayer DismissedMatchPlayer { get; set; } = null!;

        public MatchPlayer? CaughtByMatchPlayer { get; set; }

        public MatchPlayer? StumpedByMatchPlayer { get; set; }
    }
}