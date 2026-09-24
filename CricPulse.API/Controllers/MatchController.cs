using CricPulse.Application.DTOs.Match;
using CricPulse.Application.Interfaces.Match;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CricPulse.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MatchController : ControllerBase
    {
        private readonly IMatchScoringService _matchScoringService;
        private readonly IMatchService _matchService;

        public MatchController(
            IMatchScoringService matchScoringService,
            IMatchService matchService)
        {
            _matchScoringService = matchScoringService;
            _matchService = matchService;
        }

        // ====================================================================
        // HELPERS
        // ====================================================================

        private int? GetCurrentUserId()
        {
            var claim =
                User.FindFirst(
                    ClaimTypes.NameIdentifier)?.Value;

            return int.TryParse(claim, out var userId)
                ? userId
                : null;
        }

        // ====================================================================
        // MATCH MANAGEMENT
        // ====================================================================

        [HttpPost]
        public async Task<IActionResult> CreateMatch(
            [FromBody] CreateMatchDto dto)
        {
            var umpireId = GetCurrentUserId();

            if (!umpireId.HasValue)
                return Unauthorized();

            try
            {
                var result =
                    await _matchService.CreateMatchAsync(
                        umpireId.Value,
                        dto);

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAllMatches()
        {
            var matches =
                await _matchService.GetAllMatchesAsync();

            return Ok(matches);
        }

        [AllowAnonymous]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetMatch(int id)
        {
            var match =
                await _matchService.GetMatchByIdAsync(id);

            if (match == null)
                return NotFound("Match not found.");

            return Ok(match);
        }

        [AllowAnonymous]
        [HttpGet("nearby")]
        public async Task<IActionResult> GetNearbyMatches(
            [FromQuery] double latitude,
            [FromQuery] double longitude)
        {
            const double radiusInKm = 30;

            var matches =
                await _matchService.GetNearbyMatchesAsync(
                    latitude,
                    longitude,
                    radiusInKm);

            return Ok(matches);
        }

        [AllowAnonymous]
        [HttpGet("state/{state}")]
        public async Task<IActionResult> GetMatchesByState(
            string state)
        {
            var matches =
                await _matchService.GetMatchesByStateAsync(state);

            return Ok(matches);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateMatch(
            int id,
            [FromBody] UpdateMatchDto dto)
        {
            var userId = GetCurrentUserId();

            if (!userId.HasValue)
                return Unauthorized();

            try
            {
                var match =
                    await _matchService.UpdateMatchAsync(
                        id,
                        userId.Value,
                        dto);

                if (match == null)
                    return NotFound("Match not found.");

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

        [HttpPost("{id:int}/start")]
        public async Task<IActionResult> StartMatch(
            int id,
            [FromBody] StartMatchDto dto)
        {
            var userId = GetCurrentUserId();

            if (!userId.HasValue)
                return Unauthorized();

            try
            {
                var match =
                    await _matchService.StartMatchAsync(
                        id,
                        userId.Value,
                        dto);

                if (match == null)
                    return NotFound("Match not found.");

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

        [HttpPost("{id:int}/cancel")]
        public async Task<IActionResult> CancelMatch(int id)
        {
            var userId = GetCurrentUserId();

            if (!userId.HasValue)
                return Unauthorized();

            try
            {
                var result =
                    await _matchService.CancelMatchAsync(
                        id,
                        userId.Value);

                if (!result)
                    return BadRequest(
                        "Match cannot be cancelled in its current state.");

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

        [HttpGet("my-matches")]
        public async Task<IActionResult> GetMyMatches()
        {
            var umpireId = GetCurrentUserId();

            if (!umpireId.HasValue)
                return Unauthorized();

            var matches =
                await _matchService.GetMyMatchesAsync(
                    umpireId.Value);

            return Ok(matches);
        }

        [HttpGet("player-lookup")]
        public async Task<IActionResult> LookupPlayerByMobile(
            [FromQuery] string mobileNumber)
        {
            if (string.IsNullOrWhiteSpace(mobileNumber))
                return BadRequest("Mobile number is required.");

            var umpireId = GetCurrentUserId();

            if (!umpireId.HasValue)
                return Unauthorized();

            try
            {
                var result =
                    await _matchService.LookupPlayerByMobileAsync(
                        umpireId.Value,
                        mobileNumber);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("player-onboarding")]
        public async Task<IActionResult> StartPlayerOnboarding(
            [FromBody] StartPlayerOnboardingDto dto)
        {
            var umpireId = GetCurrentUserId();

            if (!umpireId.HasValue)
                return Unauthorized();

            try
            {
                var userId =
                    await _matchService.StartPlayerOnboardingAsync(
                        umpireId.Value,
                        dto.MobileNumber);

                return Ok(new
                {
                    userId,
                    message = "Player onboarding OTP sent successfully."
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("player-onboarding/verify")]
        public async Task<IActionResult> VerifyPlayerOnboarding(
            [FromBody] VerifyPlayerOnboardingDto dto)
        {
            var result =
                await _matchService.VerifyPlayerOnboardingAsync(
                    dto.UserId,
                    dto.OtpCode);

            if (!result)
                return BadRequest(
                    "Unable to verify player onboarding.");

            return Ok(new
            {
                message = "Player onboarding verified successfully."
            });
        }

        // ====================================================================
        // SCORING
        // ====================================================================

        [HttpPost("score-runs")]
        public async Task<IActionResult> ScoreRuns(
            [FromBody] ScoreBallDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid scoring request.");

            var umpireId = GetCurrentUserId();

            if (!umpireId.HasValue)
                return Unauthorized();

            var result =
                await _matchScoringService.ScoreRunsAsync(
                    umpireId.Value,
                    dto.InningsId,
                    dto.Runs);

            if (!result)
                return BadRequest(
                    "Unable to record the scoring action.");

            return Ok(new
            {
                message = "Runs recorded successfully."
            });
        }

        [HttpPost("score-wicket")]
        public async Task<IActionResult> ScoreWicket(
            [FromBody] ScoreWicketDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid wicket request.");

            var umpireId = GetCurrentUserId();

            if (!umpireId.HasValue)
                return Unauthorized();

            try
            {
                var result =
                    await _matchScoringService.ScoreWicketAsync(
                        umpireId.Value,
                        dto);

                if (!result)
                    return BadRequest(
                        "Unable to record the wicket.");

                return Ok(new
                {
                    message = "Wicket recorded successfully."
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpPost("score-extra")]
        public async Task<IActionResult> ScoreExtra(
            [FromBody] ScoreExtraDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid extra-scoring request.");

            var umpireId = GetCurrentUserId();

            if (!umpireId.HasValue)
                return Unauthorized();

            var result =
                await _matchScoringService.ScoreExtraAsync(
                    umpireId.Value,
                    dto);

            if (!result)
                return BadRequest(
                    "Unable to record the extra.");

            return Ok(new
            {
                message = "Extra recorded successfully."
            });
        }

        // ====================================================================
        // BOWLER
        // ====================================================================

        [HttpPost("change-bowler")]
        public async Task<IActionResult> ChangeBowler(
            [FromBody] ChangeBowlerDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid bowler request.");

            var umpireId = GetCurrentUserId();

            if (!umpireId.HasValue)
                return Unauthorized();

            var result =
                await _matchScoringService.ChangeBowlerAsync(
                    umpireId.Value,
                    dto.InningsId,
                    dto.NewBowlerMatchPlayerId);

            if (!result)
                return BadRequest(
                    "Unable to change the bowler.");

            return Ok(new
            {
                message = "Bowler changed successfully."
            });
        }

        // ====================================================================
        // UNDO
        // ====================================================================

        [HttpPost("undo-score")]
        public async Task<IActionResult> UndoScore(
            [FromBody] UndoScoreDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid undo request.");

            var umpireId = GetCurrentUserId();

            if (!umpireId.HasValue)
                return Unauthorized();

            var result =
                await _matchScoringService.UndoLastScoreAsync(
                    umpireId.Value,
                    dto);

            if (!result)
                return BadRequest(
                    "There is no scoring action available to undo.");

            return Ok(new
            {
                message = "Last scoring action undone successfully."
            });
        }

        // ====================================================================
        // TOSS
        // ====================================================================

        [HttpPost("record-toss")]
        public async Task<IActionResult> RecordToss(
            [FromBody] RecordTossDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid toss request.");

            var umpireId = GetCurrentUserId();

            if (!umpireId.HasValue)
                return Unauthorized();

            var result =
                await _matchScoringService.RecordTossAsync(
                    umpireId.Value,
                    dto);

            if (!result)
                return BadRequest(
                    "Unable to record toss information.");

            return Ok(new
            {
                message = "Toss recorded successfully."
            });
        }

        // ====================================================================
        // START INNINGS
        // ====================================================================

        [HttpPost("start-innings")]
        public async Task<IActionResult> StartInnings(
            [FromBody] StartInningsDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid innings request.");

            var umpireId = GetCurrentUserId();

            if (!umpireId.HasValue)
                return Unauthorized();

            var result =
                await _matchScoringService.StartInningsAsync(
                    umpireId.Value,
                    dto);

            if (!result)
                return BadRequest(
                    "Unable to start innings.");

            return Ok(new
            {
                message = "Innings started successfully."
            });
        }

        // ====================================================================
        // LIVE MATCH
        // ====================================================================

        [HttpGet("live/{matchId:int}")]
        public async Task<IActionResult> GetLiveMatch(
            int matchId)
        {
            var result =
                await _matchService.GetLiveMatchAsync(matchId);

            if (result == null)
                return NotFound(
                    "Live match information was not found.");

            return Ok(result);
        }

        // ====================================================================
        // COMPLETE MATCH
        // ====================================================================

        [HttpPost("{matchId:int}/complete")]
        public async Task<IActionResult> CompleteMatch(
            int matchId)
        {
            var umpireId = GetCurrentUserId();

            if (!umpireId.HasValue)
                return Unauthorized();

            var result =
                await _matchScoringService.CompleteMatchAsync(
                    umpireId.Value,
                    matchId);

            if (!result)
                return BadRequest(
                    "Match cannot be completed in its current state.");

            return Ok(new
            {
                message = "Match completed successfully."
            });
        }
    }
}
