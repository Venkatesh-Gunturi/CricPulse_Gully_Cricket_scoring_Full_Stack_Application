using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CricPulse.Application.DTOs.Auth;
using CricPulse.Application.DTOs.User;

namespace CricPulse.Application.Interfaces.Auth
{
    public interface IAuthService
    {
        Task<UserResponseDto> RegisterPlayerAsync(RegisterPlayerDto dto);
    }
}
