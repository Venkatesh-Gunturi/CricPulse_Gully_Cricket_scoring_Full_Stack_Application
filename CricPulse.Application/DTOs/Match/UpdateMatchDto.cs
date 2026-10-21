
namespace CricPulse.Application.DTOs.Match;

public class UpdateMatchDto
{
    public string Name { get; set; } = string.Empty;

    public string TeamAName { get; set; } = string.Empty;

    public string? TeamALogo { get; set; }

    public string TeamBName { get; set; } = string.Empty;

    public string? TeamBLogo { get; set; }

    public DateTime ScheduledAt { get; set; }

    public string Venue { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public int Overs { get; set; }

    public int PlayersPerTeam { get; set; }

    public List<MatchPlayerInputDto> Players { get; set; } = new();
}
