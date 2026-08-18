using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CricPulse.Domain.Enums;

namespace CricPulse.Domain.Entities
{
    public class OtpVerification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string OtpCodeHash { get; set; }= string.Empty;
        public OtpType OtpType { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
        public int AttemptedCount { get; set; }
        public DateTime CreatedAt { get; set; }

        //Navigation prop
        public User User { get; set; }
    }
}
