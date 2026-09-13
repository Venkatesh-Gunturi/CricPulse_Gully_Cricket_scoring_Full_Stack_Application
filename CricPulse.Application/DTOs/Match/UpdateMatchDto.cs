namespace CricPulse.Application.DTOs.Match
{
    public class UpdateMatchDto
    {
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

        public string? LiveStreamUrl { get; set; }

        public List<MatchPlayerDto> Players { get; set; } = new();
    }
}