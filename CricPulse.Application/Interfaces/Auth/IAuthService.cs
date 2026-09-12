using CricPulse.Application.DTOs.Auth;
using CricPulse.Application.DTOs.User;

namespace CricPulse.Application.Interfaces.Auth
{
    public interface IAuthService
    {
        Task<RegistrationOtpResponseDto> RegisterPlayerAsync(RegisterPlayerDto dto);

        Task<bool> VerifyOtpAsync(VerifyOtpDto dto);

        Task<LoginResponseDto?> LoginAsync(LoginDto dto);

        Task<UserResponseDto> RegisterMatchPlayerAsync(string mobileNumber);
    }
}