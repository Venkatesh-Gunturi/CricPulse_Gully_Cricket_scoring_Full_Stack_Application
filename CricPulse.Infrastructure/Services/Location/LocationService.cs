using System.Text.Json;
using CricPulse.Application.Interfaces.Location;

namespace CricPulse.Infrastructure.Services.Location
{
    public class LocationService : ILocationService
    {
        private readonly HttpClient _httpClient;

        public LocationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string?> GetStateAsync(
            double latitude,
            double longitude)
        {
            var url =
                $"https://nominatim.openstreetmap.org/reverse" +
                $"?lat={latitude}" +
                $"&lon={longitude}" +
                $"&format=json" +
                $"&addressdetails=1";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();

            using var document = JsonDocument.Parse(json);

            if (!document.RootElement.TryGetProperty(
                    "address",
                    out var address))
            {
                return null;
            }

            if (!address.TryGetProperty(
                    "state",
                    out var state))
            {
                return null;
            }

            return state.GetString();
        }
    }
}