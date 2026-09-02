using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CricPulse.Domain.Entities
{
    public class Player
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }=string.Empty;
        public string BattingStyle { get; set; }    = string.Empty;
        public string BowlingStyle { get; set; }= string.Empty;
        public string PlayerRole { get; set; } = string.Empty;
        public string  State { get; set; } = string.Empty;
        public int PinCode { get; set; }

        public User User { get; set; } = null!;

    }
}
