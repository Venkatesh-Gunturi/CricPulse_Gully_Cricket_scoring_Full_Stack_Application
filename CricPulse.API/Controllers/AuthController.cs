using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using CricPulse.Application.DTOs.Auth;
using CricPulse.Application.DTOs.User;
using CricPulse.Application.Interfaces.Auth;


namespace CricPulse.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserResponseDto>> Register(RegisterPlayerDto dto)
        {
            var user = await _authService.RegisterPlayerAsync(dto);

            return StatusCode(StatusCodes.Status201Created,user);
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp(VerifyOtpDto dto)
        {
            var result = await _authService.VerifyOtpAsync(dto);

            if (!result)
            {
                return BadRequest(new
                {
                    message = "Invalid or expired OTP."
                });
            }

            return Ok(new
            {
                message = "OTP verified successfully."
            });
        }
    }
}
