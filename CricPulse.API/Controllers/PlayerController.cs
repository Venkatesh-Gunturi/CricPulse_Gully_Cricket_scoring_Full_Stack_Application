using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CricPulse.Application.DTOs.Player;
using CricPulse.Application.Interfaces.Player;

namespace CricPulse.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PlayerController : ControllerBase
    {
        private readonly IPlayerService _playerService;

        public PlayerController(IPlayerService playerService)
        {
            _playerService = playerService;
        }

        // ============================================================
        // CREATE PLAYER PROFILE
        // ============================================================

        [HttpPost("profile")]
        public async Task<IActionResult> CreateProfile(
            CreatePlayerDto dto)
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            if (!int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                var player =
                    await _playerService.CreateProfileAsync(
                        userId,
                        dto);

                return Ok(player);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ============================================================
        // GET PLAYER PROFILE + STATISTICS
        // ============================================================

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            if (!int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized();
            }

            var playerProfile =
                await _playerService.GetPlayerProfileAsync(userId);

            if (playerProfile == null)
            {
                return NotFound("Player profile not found.");
            }

            return Ok(playerProfile);
        }

        // ============================================================
        // UPDATE PLAYER PROFILE
        // ============================================================

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(
            CreatePlayerDto dto)
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            if (!int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                var player =
                    await _playerService.UpdateProfileAsync(
                        userId,
                        dto);

                return Ok(player);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}