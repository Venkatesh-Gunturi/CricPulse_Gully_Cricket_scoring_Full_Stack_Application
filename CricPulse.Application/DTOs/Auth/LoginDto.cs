using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CricPulse.Application.DTOs.Auth
{
    public class LoginDto
    {
        public required string Identifier { get; set; }

        public required string Password { get; set; }
    }
}
