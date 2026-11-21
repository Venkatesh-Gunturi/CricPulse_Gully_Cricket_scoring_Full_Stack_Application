using CricPulse.Application.DTOs.Auth;
using CricPulse.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace CricPulse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IForgotPasswordService _forgotPasswordService;

    public AuthController(
        IAuthService authService,
        IForgotPasswordService forgotPasswordService)
    {
        _authService = authService;
        _forgotPasswordService = forgotPasswordService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        try
        {
            var result = await _authService.RegisterAsync(dto);
            return Ok(result);
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

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginDto dto,
        [FromServices] IOptionsMonitor<BearerTokenOptions> bearerOptions)
    {
        try
        {
            var result = await _authService.LoginAsync(dto);
            var scheme = BearerTokenDefaults.AuthenticationScheme;
            var options = bearerOptions.Get(scheme);
            var identity = new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, result.UserId.ToString()) },
                scheme);
            var properties = new AuthenticationProperties
            {
                ExpiresUtc = DateTimeOffset.UtcNow.Add(options.BearerTokenExpiration)
            };
            var ticket = new AuthenticationTicket(
                new ClaimsPrincipal(identity), properties, scheme);
            result.AccessToken = options.BearerTokenProtector.Protect(ticket);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }


    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
         ForgotPasswordDto dto)
    {
        try
        {
            await _forgotPasswordService
                .RequestPasswordResetAsync(dto);

            return Ok(new
            {
                message = "Password reset successful."
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }
}