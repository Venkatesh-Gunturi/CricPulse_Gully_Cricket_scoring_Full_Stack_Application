using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;
namespace CricPulse.Application.DTOs.Auth
{
    public class RegisterPlayerDto
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }  = string.Empty;

        [StringLength(100)]
        public string? LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }= string.Empty;

        [Required]
        [RegularExpression(@"^[0-9]{10}$",
             ErrorMessage = "Mobile number must contain exactly 10 digits.")]
        public string MobileNumber { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string Password { get; set; }=string.Empty;
    }
}
