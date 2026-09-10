using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CricPulse.Domain.Entities
{
    public class Match
    {
        public int Id { get; set; }

        public int UmpireId { get; set; }

        public string Team1Name { get; set; } = string.Empty;
        public string Team1Logo { get; set; } = string.Empty;

        public string Team2Name { get; set; } = string.Empty;
        public string Team2Logo { get; set; } = string.Empty;

        public int PlayersPerTeam { get; set; }
        public int Overs { get; set; }

        public DateTime MatchDate { get; set; }
        public TimeSpan MatchTime { get; set; }

        //Location details 
        public string VenueName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string State { get; set; } = string.Empty;
        

        public string? LiveStreamUrl { get; set; }

        public string Status { get; set; } = "Scheduled";

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public User Umpire { get; set; } = null!;

        public ICollection<MatchPlayer> MatchPlayers { get; set; }= new List<MatchPlayer>();

        public ICollection<Innings> Innings { get; set; }
    = new List<Innings>();

    }
}
