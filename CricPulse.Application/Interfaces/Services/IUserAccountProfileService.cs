
using CricPulse.Application.DTOs.Player;

namespace CricPulse.Application.Interfaces.Services;

public interface IUserAccountProfileService
{
    Task<UserProfileResponseDto> GetProfileAsync(Guid userId);

    Task<UserProfileResponseDto> UpdateProfileAsync(
        Guid userId,
        UpdateUserProfileDto dto);
}
