using CricPulse.Application.DTOs.Match;
using CricPulse.Application.Interfaces;
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
        private readonly IMatchScoringService _matchScoringService;

        //dependencies
        public MatchController(
            IMatchService matchService,
            ILocationService locationService,
            IMatchScoringService matchScoringService)
        {
            _matchService = matchService;
            _locationService = locationService;
            _matchScoringService= matchScoringService;
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


        // Purpose:
        // Look up a player while ensuring the authenticated umpire cannot select themselves.
        [Authorize]
        [HttpGet("player-lookup")]
        public async Task<IActionResult> LookupPlayer(
            [FromQuery] string mobileNumber)
        {
            if (string.IsNullOrWhiteSpace(mobileNumber))
            {
                return BadRequest("Mobile number is required.");
            }

            var umpireId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result =
                await _matchService.LookupPlayerByMobileAsync(
                    umpireId,
                    mobileNumber);

            return Ok(result);
        }


        // Purpose:
        // Start player OTP onboarding while preventing the authenticated umpire
        // from onboarding themselves as a player.
        [Authorize]
        [HttpPost("player-onboarding")]
        public async Task<IActionResult> StartPlayerOnboarding(
            [FromBody] StartPlayerOnboardingDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.MobileNumber))
            {
                return BadRequest("Mobile number is required.");
            }

            var umpireId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var userId =
                await _matchService.StartPlayerOnboardingAsync(
                    umpireId,
                    dto.MobileNumber);

            return Ok(new
            {
                userId,
                message = "OTP generated successfully."
            });
        }


        [Authorize]
        [HttpPost("player-onboarding/verify")]
        public async Task<IActionResult> VerifyPlayerOnboarding(
    [FromBody] VerifyPlayerOnboardingDto dto)
        {
            var result =
                await _matchService.VerifyPlayerOnboardingAsync(
                    dto.UserId,
                    dto.OtpCode);

            if (!result)
            {
                return BadRequest("Invalid or expired OTP.");
            }

            return Ok(new
            {
                message = "Player onboarding completed successfully."
            });
        }


        // Purpose:
        // Record a normal legal delivery and update the innings score and strike state.
        [Authorize]
        [HttpPost("score-runs")]
        public async Task<IActionResult> ScoreRuns([FromBody] ScoreBallDto dto)
        {
            var umpireId = int.Parse(
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var result = await _matchScoringService
                .ScoreRunsAsync(umpireId, dto.InningsId, dto.Runs);

            return result
                ? Ok()
                : BadRequest("Unable to score runs.");
        }

        // Purpose:
        // Record a wicket delivery, including dismissal details and the new batter.
        [Authorize]
        [HttpPost("score-wicket")]
        public async Task<IActionResult> ScoreWicket([FromBody] ScoreWicketDto dto)
        {
            var umpireId = int.Parse(
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var result = await _matchScoringService
                .ScoreWicketAsync(umpireId, dto);

            return result
                ? Ok()
                : BadRequest("Unable to score wicket.");
        }

        // Purpose:
        // Record an extra such as wide, no-ball, bye, or leg bye.
        [Authorize]
        [HttpPost("score-extra")]
        public async Task<IActionResult> ScoreExtra([FromBody] ScoreExtraDto dto)
        {
            var umpireId = int.Parse(
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var result = await _matchScoringService
                .ScoreExtraAsync(umpireId, dto);

            return result
                ? Ok()
                : BadRequest("Unable to score extra.");
        }

        // Purpose:
        // Undo only the immediately previous scoring action.
        [Authorize]
        [HttpPost("undo-score")]
        public async Task<IActionResult> UndoScore([FromBody] UndoScoreDto dto)
        {
            var umpireId = int.Parse(
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var result = await _matchScoringService
                .UndoLastScoreAsync(umpireId, dto);

            return result
                ? Ok()
                : BadRequest("Unable to undo score.");
        }


        // Purpose:
        // Accept the umpire's final toss decision and record which team bats first.
        [Authorize]
        [HttpPost("record-toss")]
        public async Task<IActionResult> RecordToss(
            [FromBody] RecordTossDto dto)
        {
            var umpireId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = await _matchScoringService
                .RecordTossAsync(umpireId, dto);

            return result
                ? Ok()
                : BadRequest("Unable to record toss.");
        }


        // Purpose:
        // Start the first innings with the umpire-selected opening batters and bowler.
        [Authorize]
        [HttpPost("start-innings")]
        public async Task<IActionResult> StartInnings(
            [FromBody] StartInningsDto dto)
        {
            var umpireId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = await _matchScoringService
                .StartInningsAsync(umpireId, dto);

            return result
                ? Ok()
                : BadRequest("Unable to start innings.");
        }

        // Purpose:
        // Start the match when the assigned umpire chooses to begin it.
        [Authorize]
        [HttpPost("start-match/{matchId}")]
        public async Task<IActionResult> StartMatch(int matchId)
        {
            var umpireId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = await _matchScoringService
                .StartMatchAsync(umpireId, matchId);

            return result
                ? Ok()
                : BadRequest("Unable to start match.");
        }

        // Purpose:
        // Return the current live match state for the requested match.
        [Authorize]
        [HttpGet("live/{matchId}")]
        public async Task<IActionResult> GetLiveMatch(int matchId)
        {
            var match = await _matchService
                .GetLiveMatchAsync(matchId);

            if (match == null)
            {
                return NotFound("Live match not found.");
            }

            return Ok(match);
        }
    }
}
