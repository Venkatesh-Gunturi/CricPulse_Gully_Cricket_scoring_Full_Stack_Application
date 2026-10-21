using System.Globalization;
using System.Text.Json;
using CricPulse.Application.Interfaces.Services;
using Microsoft.Extensions.Caching.Memory;

namespace CricPulse.Infrastructure.Services.Location;

public sealed class NominatimReverseGeocodingService
    : IReverseGeocodingService
{
    private static readonly SemaphoreSlim RequestGate = new(1, 1);
    private static DateTime _lastRequestUtc = DateTime.MinValue;

    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;

    public NominatimReverseGeocodingService(
        HttpClient httpClient,
        IMemoryCache cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    public async Task<ResolvedLocation?> ResolveAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default)
    {
        if (latitude is < -90 or > 90 ||
            longitude is < -180 or > 180)
        {
            throw new ArgumentOutOfRangeException(
                nameof(latitude),
                "Coordinates are outside valid geographic bounds.");
        }

        // Round coordinates for cache reuse without storing a map link.
        var cacheKey = string.Format(
            CultureInfo.InvariantCulture,
            "nominatim:{0:F4}:{1:F4}",
            latitude,
            longitude);

        if (_cache.TryGetValue(cacheKey, out ResolvedLocation? cached))
            return cached;

        await RequestGate.WaitAsync(cancellationToken);

        try
        {
            // Public Nominatim: no more than one request per second
            // across this application's service instances.
            var elapsed = DateTime.UtcNow - _lastRequestUtc;

            if (elapsed < TimeSpan.FromSeconds(1))
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(1) - elapsed,
                    cancellationToken);
            }

            _lastRequestUtc = DateTime.UtcNow;

            var url = string.Format(
                CultureInfo.InvariantCulture,
                "reverse?format=jsonv2&addressdetails=1&lat={0}&lon={1}",
                latitude,
                longitude);

            using var response = await _httpClient.GetAsync(
                url,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            await using var stream =
                await response.Content.ReadAsStreamAsync(cancellationToken);

            using var document = await JsonDocument.ParseAsync(
                stream,
                cancellationToken: cancellationToken);

            var root = document.RootElement;

            if (!root.TryGetProperty("address", out var address))
                return null;

            string? ReadAddressValue(string property)
            {
                return address.TryGetProperty(property, out var value)
                    ? value.GetString()
                    : null;
            }

            var displayAddress = root.TryGetProperty(
                "display_name", out var displayName)
                ? displayName.GetString()
                : null;

            var state = ReadAddressValue("state");

            var result = new ResolvedLocation
            {
                Address = displayAddress,
                State = state,
                PinCode = ReadAddressValue("postcode")
            };

            _cache.Set(
                cacheKey,
                result,
                TimeSpan.FromDays(30));

            return result;
        }
        finally
        {
            RequestGate.Release();
        }
    }
}