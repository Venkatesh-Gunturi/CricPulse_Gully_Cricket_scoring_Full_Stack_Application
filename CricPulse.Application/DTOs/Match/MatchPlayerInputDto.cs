namespace CricPulse.Application.DTOs.Match;

public class MatchPlayerInputDto
{
    public string Team { get; set; } = string.Empty;

    public string? MobileNumber { get; set; }

    public string? Email { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }
}