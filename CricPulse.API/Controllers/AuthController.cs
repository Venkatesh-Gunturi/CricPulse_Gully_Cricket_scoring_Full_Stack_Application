using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using CricPulse.Application.DTOs.Auth;
using CricPulse.Application.DTOs.User;
using CricPulse.Application.Interfaces.Auth;
using Microsoft.AspNetCore.Mvc;

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

    }
}
