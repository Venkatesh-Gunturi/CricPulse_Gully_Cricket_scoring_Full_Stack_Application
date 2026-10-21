namespace CricPulse.Application.DTOs.Player;

public sealed class ValidateRosterPlayerDto
{
    public Guid CreatedByUserId { get; set; }

    public string Identifier { get; set; } = string.Empty;

    public List<Guid> ExistingUserIds { get; set; } = [];

    public List<string> ExistingIdentifiers { get; set; } = [];
}