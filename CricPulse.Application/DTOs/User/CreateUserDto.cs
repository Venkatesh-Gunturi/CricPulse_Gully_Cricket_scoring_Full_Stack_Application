using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CricPulse.Application.DTOs.User
{
    public class CreateUserDto
    {
        public string FirstName { get; set; }   = string.Empty;
        public string? LastName { get; set; }
        public string Email { get; set; }= string.Empty;
        public string MobileNumber { get; set; }=string.Empty;

    }
}
