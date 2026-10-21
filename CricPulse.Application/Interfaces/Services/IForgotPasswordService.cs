using CricPulse.Application.DTOs.Auth;

namespace CricPulse.Application.Interfaces.Services;

public interface IForgotPasswordService
{
    Task RequestPasswordResetAsync(ForgotPasswordDto dto);
}