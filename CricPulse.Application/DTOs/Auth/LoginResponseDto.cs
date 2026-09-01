using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CricPulse.Application.DTOs.User;

namespace CricPulse.Application.DTOs.Auth
{
    public class LoginResponseDto
    {
        public string Token { get; set; }=string.Empty;
        public UserResponseDto User { get; set; } = null!;
    }
}
