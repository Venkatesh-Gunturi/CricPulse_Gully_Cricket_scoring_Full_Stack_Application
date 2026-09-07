using MatchEntity= CricPulse.Domain.Entities.Match;

namespace CricPulse.Application.Interfaces.Match
{
    public class NearbyMatchResult
    {
        public MatchEntity Match { get; set; } = null!;
        public double DistanceInKm { get; set; }
    }
}