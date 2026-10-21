
using CricPulse.Application.DTOs.Player;
using CricPulse.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace CricPulse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountProfileController : ControllerBase
{
    private readonly IUserAccountProfileService _profileService;

    public AccountProfileController(
        IUserAccountProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetProfile(Guid userId)
    {
        try
        {
            return Ok(await _profileService.GetProfileAsync(userId));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("{userId:guid}")]
    public async Task<IActionResult> UpdateProfile(
        Guid userId,
        [FromBody] UpdateUserProfileDto dto)
    {
        try
        {
            var result = await _profileService.UpdateProfileAsync(
                userId, dto);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
