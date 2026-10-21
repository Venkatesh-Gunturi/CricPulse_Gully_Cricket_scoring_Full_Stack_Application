
using CricPulse.Application.DTOs.Player;
using CricPulse.Application.Services.Player;
using Microsoft.AspNetCore.Mvc;

namespace CricPulse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayerLookupController : ControllerBase
{
    private readonly PlayerLookupService _playerLookupService;

    public PlayerLookupController(
        PlayerLookupService playerLookupService)
    {
        _playerLookupService = playerLookupService;
    }

    [HttpGet]
    public async Task<IActionResult> Find(
        [FromQuery] string? mobileNumber,
        [FromQuery] string? email)
    {
        try
        {
            var player = await _playerLookupService
                .FindByIdentifierAsync(mobileNumber, email);

            if (player is null)
                return NotFound(new { message = "No registered account found." });

            return Ok(player);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("onboard")]
    public async Task<IActionResult> Onboard(
        [FromBody] OnboardPlayerDto dto)
    {
        try
        {
            var player = await _playerLookupService.OnboardAsync(dto);
            return Ok(player);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("validate-roster-player")]
    public async Task<IActionResult> ValidateRosterPlayer(
        [FromBody] ValidateRosterPlayerDto dto)
    {
        try
        {
            var player = await _playerLookupService
                .ValidateRosterPlayerAsync(dto, dto.CreatedByUserId);

            if (player is null)
            {
                return NotFound(new
                {
                    requiresOnboarding = true,
                    message = "No registered account found. Enter the player's mobile number, email, and name to onboard them."
                });
            }

            return Ok(new { isValid = true, requiresOnboarding = false, player });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
