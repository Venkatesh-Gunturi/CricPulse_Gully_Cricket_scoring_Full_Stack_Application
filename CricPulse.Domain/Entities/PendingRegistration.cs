namespace CricPulse.Domain.Entities
{
    public class PendingRegistration
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string? LastName { get; set; }

        public string MobileNumber { get; set; } = string.Empty;

        public string? PasswordHash { get; set; }

        public string OtpCode { get; set; } = string.Empty;

        public DateTime OtpExpiresAt { get; set; }

        public int AttemptedCount { get; set; }
        public bool IsOtpVerified { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}