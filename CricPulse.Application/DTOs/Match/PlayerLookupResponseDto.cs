namespace CricPulse.Application.DTOs.Match
{
    public class PlayerLookupResponseDto
    {
        public bool IsRegistered { get; set; }

        public int? PlayerId { get; set; }

        public string DisplayName { get; set; } = string.Empty;
    }
}