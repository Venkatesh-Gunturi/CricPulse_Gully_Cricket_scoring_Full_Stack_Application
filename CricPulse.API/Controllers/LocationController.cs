using CricPulse.Application.Interfaces.Location;
using Microsoft.AspNetCore.Mvc;

namespace CricPulse.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationController : ControllerBase
    {
        private readonly ILocationService _locationService;

        public LocationController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        [HttpGet("state")]
        public async Task<IActionResult> GetState(
            double latitude,
            double longitude)
        {
            var state = await _locationService.GetStateAsync(
                latitude,
                longitude);

            if (string.IsNullOrEmpty(state))
            {
                return NotFound("Unable to determine state.");
            }

            return Ok(state);
        }
    }
}