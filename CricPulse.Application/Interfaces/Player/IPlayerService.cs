using CricPulse.Application.Interfaces.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CricPulse.Application.DTOs.Player;

namespace CricPulse.Application.Interfaces.Player
{
    public interface IPlayerService
    {
        Task<PlayerResponseDto> CreateProfileAsync(int userId, CreatePlayerDto dto);

        Task<PlayerResponseDto?> GetProfileAsync(int userId);

        Task<PlayerResponseDto> UpdateProfileAsync(int userId, CreatePlayerDto dto);


    }

}
