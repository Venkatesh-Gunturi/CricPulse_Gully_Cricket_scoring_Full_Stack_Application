namespace CricPulse.Application.Utilities
{
    public static class MatchTimeHelper
    {
        private static readonly TimeZoneInfo IndiaTimeZone =
            TimeZoneInfo.FindSystemTimeZoneById(
                OperatingSystem.IsWindows()
                    ? "India Standard Time"
                    : "Asia/Kolkata");

        // Purpose:
        // Convert the match's stored IST date and time into UTC for deadline comparisons.
        public static DateTime GetScheduledUtc(
            DateTime matchDate,
            TimeSpan matchTime)
        {
            var indiaDateTime = DateTime.SpecifyKind(
                matchDate.Date.Add(matchTime),
                DateTimeKind.Unspecified);

            return TimeZoneInfo.ConvertTimeToUtc(
                indiaDateTime,
                IndiaTimeZone);
        }
    }
}