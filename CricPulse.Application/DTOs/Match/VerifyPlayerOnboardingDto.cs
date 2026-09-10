namespace CricPulse.Application.DTOs.Match
{
    public class VerifyPlayerOnboardingDto
    {
        public int UserId { get; set; }

        public string OtpCode { get; set; } = string.Empty;
    }
}