using System;

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

        public int BatterRuns { get; set; }

        public bool IsLegalDelivery { get; set; }

        public string? ExtraType { get; set; }

        public int ExtraRuns { get; set; }

        public string? Notation { get; set; }

        // ================================================================
        // PREVIOUS STATE SNAPSHOT
        // ================================================================

        public int PreviousStrikerMatchPlayerId { get; set; }

        public int PreviousNonStrikerMatchPlayerId { get; set; }

        public int? PreviousBowlerMatchPlayerId { get; set; }

        public int PreviousTotalRuns { get; set; }

        public int PreviousWickets { get; set; }

        public int PreviousLegalBalls { get; set; }

        public string PreviousInningsStatus { get; set; } = string.Empty;

        public bool PreviousFreeHit { get; set; }

        public string PreviousMatchStatus { get; set; } = string.Empty;

        public string PreviousMatchResult { get; set; } = string.Empty;

        public DateTime? PreviousCompletionDeadline { get; set; }

        // ================================================================
        // UNDO
        // ================================================================

        /// <summary>
        /// Only the most recent scoring action is undoable.
        /// Once another scoring action is recorded, this becomes false
        /// permanently for the previous ball.
        /// </summary>
        public bool CanUndo { get; set; }

        // ================================================================
        // FREE HIT
        // ================================================================

        /// <summary>
        /// Indicates whether the next delivery is a free hit
        /// after this delivery.
        /// </summary>
        public bool FreeHitAfterDelivery { get; set; }

        // ================================================================
        // AUDIT
        // ================================================================

        public DateTime CreatedAt { get; set; }

        // ================================================================
        // NAVIGATION PROPERTIES
        // ================================================================

        public Innings Innings { get; set; } = null!;

        public MatchPlayer StrikerMatchPlayer { get; set; } = null!;

        public MatchPlayer NonStrikerMatchPlayer { get; set; } = null!;

        public MatchPlayer BowlerMatchPlayer { get; set; } = null!;

        public Wicket? Wicket { get; set; }
    }
}