using CricPulse.Application.DTOs.Player;

namespace CricPulse.Application.Interfaces.Player
{
    public interface IPlayerService
    {
        Task<PlayerResponseDto> CreateProfileAsync(
            int userId,
            CreatePlayerDto dto);

        Task<PlayerProfileResponseDto?> GetPlayerProfileAsync(
            int userId);

        Task<PlayerResponseDto> UpdateProfileAsync(
            int userId,
            CreatePlayerDto dto);
    }
}