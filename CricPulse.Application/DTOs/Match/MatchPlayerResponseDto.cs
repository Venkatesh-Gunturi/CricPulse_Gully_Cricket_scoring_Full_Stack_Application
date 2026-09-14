namespace CricPulse.Application.DTOs.Match
{
    public class MatchPlayerResponseDto
    {
        public int MatchPlayerId { get; set; }

        public int PlayerId { get; set; }

        public string MobileNumber { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string Team { get; set; } = string.Empty;
    }
}