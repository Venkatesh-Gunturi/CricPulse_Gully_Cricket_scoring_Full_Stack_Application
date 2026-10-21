using CricPulse.Application.DTOs.Match;
using CricPulse.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace CricPulse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MatchController : ControllerBase
{
    private readonly IMatchService _matchService;

    public MatchController(IMatchService matchService)
    {
        _matchService = matchService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create(
        [FromBody] CreateMatchDto dto)
    {
        try
        {
            var result =
                await _matchService.CreateMatchAsync(dto);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetMatches(
        [FromQuery] MatchDiscoveryQueryDto query)
    {
        try
        {
            var matches =
                await _matchService.GetMatchesAsync(query);

            return Ok(matches);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("states")]
    public async Task<IActionResult> GetStates()
    {
        var states =
            await _matchService.GetIndianStatesAsync();

        return Ok(states);
    }


    [HttpGet("mine")]
    public async Task<IActionResult> GetMyMatches(
        [FromQuery] Guid createdByUserId,
        [FromQuery] string? status)
    {
        try
        {
            var matches = await _matchService.GetMyMatchesAsync(
                createdByUserId,
                status);

            return Ok(matches);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }


    [HttpGet("{matchId:guid}")]
    public async Task<IActionResult> GetMatchById(
        Guid matchId,
        [FromQuery] Guid createdByUserId)
    {
        try
        {
            var match = await _matchService.GetMatchByIdAsync(
                matchId,
                createdByUserId);

            if (match == null)
                return NotFound(new { message = "Match not found." });

            return Ok(match);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    [HttpPut("{matchId:guid}")]
    public async Task<IActionResult> UpdateMatch(
        Guid matchId,
        [FromQuery] Guid createdByUserId,
        [FromBody] UpdateMatchDto dto)
    {
        try
        {
            var updatedMatch = await _matchService.UpdateMatchAsync(
                matchId,
                createdByUserId,
                dto);

            return Ok(updatedMatch);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{matchId:guid}/cancel")]
    public async Task<IActionResult> CancelMatch(
        Guid matchId,
        [FromQuery] Guid createdByUserId,
        [FromBody] CancelMatchDto dto)
    {
        try
        {
            await _matchService.CancelMatchAsync(
                matchId,
                createdByUserId,
                dto);

            return Ok(new
            {
                message = "Match cancelled successfully."
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

}