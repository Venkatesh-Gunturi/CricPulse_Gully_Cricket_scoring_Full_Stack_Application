using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace CricPulse.Application.DTOs.User
{
    public class CreateUserDto
    {
        [Required]
        [StringLength(50, MinimumLength =2)]
        public string FirstName { get; set; }   = string.Empty;

        [StringLength(50)]
        public string? LastName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; }= string.Empty;

        [Required]
        [RegularExpression(@"^[0-9]{10}$",
                ErrorMessage ="Mobile number must contain exactly 10 digits.")]
        public string MobileNumber { get; set; }=string.Empty;

    }
}
