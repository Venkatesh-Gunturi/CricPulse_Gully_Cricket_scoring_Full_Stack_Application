using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CricPulse.Application.DTOs.User
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Firstname { get; set; }   =string.Empty;
        public string? LastName { get; set; }
        public string Email { get; set; }=string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public bool IsEmailVerified { get; set; }
        public bool IsMobileVerified { get; set; }
        public bool IsUmpire { get; set; }
        public string? ProfileImageUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
