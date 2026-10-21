using CricPulse.Application.DTOs.Scoring;
using CricPulse.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace CricPulse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScoringController : ControllerBase
{
    private readonly IScoringService _scoringService;

    public ScoringController(IScoringService scoringService)
    {
        _scoringService = scoringService;
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartMatch(
        [FromBody] StartMatchDto dto)
    {
        try
        {
            return Ok(
                await _scoringService.StartMatchAsync(dto));
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

    [HttpPost("{matchId:guid}/toss")]
    public async Task<IActionResult> Toss(
        Guid matchId,
        [FromBody] TossDto dto)
    {
        try
        {
            return Ok(
                await _scoringService.RecordTossAsync(
                    matchId,
                    dto));
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

    [HttpPost("{matchId:guid}/innings/setup")]
    public async Task<IActionResult> SetupInnings(
        Guid matchId,
        [FromBody] SetupInningsDto dto)
    {
        try
        {
            return Ok(
                await _scoringService.SetupInningsAsync(
                    matchId,
                    dto));
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

    [HttpPost("innings/{inningsId:guid}/runs")]
    public async Task<IActionResult> Runs(
        Guid inningsId,
        [FromBody] ScoreRunDto dto)
    {
        try
        {
            return Ok(
                await _scoringService.ScoreRunsAsync(
                    inningsId,
                    dto));
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

    [HttpPost("innings/{inningsId:guid}/extra")]
    public async Task<IActionResult> Extra(
        Guid inningsId,
        [FromBody] ScoreExtraDto dto)
    {
        try
        {
            return Ok(
                await _scoringService.ScoreExtraAsync(
                    inningsId,
                    dto));
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

    [HttpPost("innings/{inningsId:guid}/wicket")]
    public async Task<IActionResult> Wicket(
        Guid inningsId,
        [FromBody] ScoreWicketDto dto)
    {
        try
        {
            return Ok(
                await _scoringService.ScoreWicketAsync(
                    inningsId,
                    dto));
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

    [HttpPost("innings/{inningsId:guid}/complete")]
    public async Task<IActionResult> CompleteInnings(
        Guid inningsId,
        [FromBody] CompleteInningsDto dto)
    {
        try
        {
            return Ok(
                await _scoringService.CompleteInningsAsync(
                    inningsId,
                    dto));
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

    [HttpPost("innings/{inningsId:guid}/undo")]
    public async Task<IActionResult> Undo(
        Guid inningsId,
        [FromBody] Guid createdByUserId)
    {
        try
        {
            return Ok(
                await _scoringService.UndoLastBallAsync(
                    inningsId,
                    createdByUserId));
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

    [HttpPost("innings/{inningsId:guid}/bowler")]
    public async Task<IActionResult> ChangeBowler(
    Guid inningsId,
    [FromBody] ChangeBowlerDto dto)
    {
        return Ok(
            await _scoringService.ChangeBowlerAsync(
                inningsId,
                dto));
    }

    [HttpGet("{matchId:guid}/state")]
    public async Task<IActionResult> GetMatchState(Guid matchId)
    {
        try
        {
            return Ok(
                await _scoringService.GetMatchStateAsync(matchId));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

}