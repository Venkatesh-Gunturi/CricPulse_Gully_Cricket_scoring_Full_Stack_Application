namespace CricPulse.Application.Interfaces.Services;

public interface IReverseGeocodingService
{
    Task<ResolvedLocation?> ResolveAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default);
}

public sealed class ResolvedLocation
{
    public string? Address { get; init; }
    public string? State { get; init; }
    public string? PinCode { get; init; }
}