using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CricPulse.Application.DTOs.Player
{
    public class CreatePlayerDto
    {
        public DateTime DateOfBirth { get; set; }

        public string Gender { get; set; } = string.Empty;

        public string BattingStyle { get; set; } = string.Empty;

        public string BowlingStyle { get; set; } = string.Empty;

        public string PlayerRole { get; set; } = string.Empty;

        public string State { get; set; } = string.Empty;

        public int PinCode { get; set; }
    }
}
