namespace CricPulse.Domain.Entities;

public class Wicket
{
    public Guid Id { get; set; }

    public Guid BallId { get; set; }

    public Guid DismissedMatchPlayerId { get; set; }

    public string WicketType { get; set; } = string.Empty;

    public Guid? CaughtByMatchPlayerId { get; set; }

    public Guid? RunOutByMatchPlayerId { get; set; }

    public Guid? StumpedByMatchPlayerId { get; set; }

    public int RunsCompleted { get; set; }

    public Ball Ball { get; set; } = null!;

    public MatchPlayer DismissedMatchPlayer { get; set; } = null!;

    public MatchPlayer? CaughtByMatchPlayer { get; set; }

    public MatchPlayer? RunOutByMatchPlayer { get; set; }

    public MatchPlayer? StumpedByMatchPlayer { get; set; }
}