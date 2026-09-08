using CricPulse.Application.DTOs.Match;
using CricPulse.Application.Interfaces.Location;
using CricPulse.Application.Interfaces.Match;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CricPulse.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class MatchController : ControllerBase
    {
        private readonly IMatchService _matchService;

        private readonly ILocationService _locationService;

        public MatchController(
            IMatchService matchService,
            ILocationService locationService)
        {
            _matchService = matchService;
            _locationService = locationService;
        }

        //Match Creation
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateMatch(CreateMatchDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim.Value);

            try
            {
                var match = await _matchService.CreateMatchAsync(
                    userId,
                    dto);

                return Ok(match);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        //Fetch match by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMatch(int id)
        {
            var match = await _matchService.GetMatchByIdAsync(id);

            if (match == null)
            {
                return NotFound("Match not found.");
            }

            return Ok(match);
        }

        //fetch All Matches
        [HttpGet]
        public async Task<IActionResult> GetAllMatches()
        {
            var matches = await _matchService.GetAllMatchesAsync();

            return Ok(matches);
        }

        [HttpGet("nearby")]
        public async Task<IActionResult> GetNearbyMatches(
        double latitude,
        double longitude)
        {
            const double radiusInKm = 30;

            var matches = await _matchService.GetNearbyMatchesAsync(
                latitude,
                longitude,
                radiusInKm);

            return Ok(matches);
        }

        [HttpGet("state/{state}")]
        public async Task<IActionResult> GetMatchesByState(string state)
        {
            var matches = await _matchService.GetMatchesByStateAsync(state);

            return Ok(matches);
        }


        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMatch(
    int id,
    UpdateMatchDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim.Value);

            try
            {
                var match = await _matchService.UpdateMatchAsync(
                    id,
                    userId,
                    dto);

                if (match == null)
                {
                    return NotFound("Match not found.");
                }

                return Ok(match);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("{id}/start")]
        public async Task<IActionResult> StartMatch(
    int id,
    StartMatchDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim.Value);

            try
            {
                var match = await _matchService.StartMatchAsync(
                    id,
                    userId,
                    dto);

                if (match == null)
                {
                    return NotFound("Match not found.");
                }

                return Ok(match);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelMatch(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim.Value);

            try
            {
                var cancelled = await _matchService.CancelMatchAsync(
                    id,
                    userId);

                if (!cancelled)
                {
                    return NotFound("Match not found.");
                }

                return Ok(new
                {
                    message = "Match cancelled successfully."
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [Authorize]
        [HttpGet("my-matches")]
        public async Task<IActionResult> GetMyMatches()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim.Value);

            var matches = await _matchService.GetMyMatchesAsync(userId);

            return Ok(matches);
        }
    }
}
