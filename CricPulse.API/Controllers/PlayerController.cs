using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CricPulse.Application.DTOs.Player;
using CricPulse.Application.Interfaces.Player;
using Microsoft.AspNetCore.Authorization;

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

        [HttpPost("profile")]
        public async Task<IActionResult> CreateProfile(CreatePlayerDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim.Value);

            try
            {
                var player = await _playerService.CreateProfileAsync(
                    userId,
                    dto);

                return Ok(player);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim.Value);

            var player = await _playerService.GetProfileAsync(userId);

            if (player == null)
            {
                return NotFound("Player profile not found.");
            }

            return Ok(player);
        }


        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(CreatePlayerDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim.Value);

            try
            {
                var player = await _playerService.UpdateProfileAsync(
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
