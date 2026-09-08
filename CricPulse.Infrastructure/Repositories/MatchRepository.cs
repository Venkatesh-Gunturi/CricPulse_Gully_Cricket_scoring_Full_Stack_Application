using CricPulse.Application.Interfaces.Match;
using CricPulse.Domain.Entities;
using CricPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CricPulse.Infrastructure.Repositories
{
    public class MatchRepository : IMatchRepository
    {
        private readonly CricPulseDbContext _context;

        public MatchRepository(CricPulseDbContext context)
        {
            _context = context;
        }

        public async Task<Match> CreateAsync(Match match)
        {
            await _context.Matches.AddAsync(match);
            await _context.SaveChangesAsync();

            return match;
        }

        public async Task<Match?> GetByIdAsync(int id)
        {
            return await _context.Matches
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        //All matches are fetching to display on Home page
        public async Task<List<Match>> GetAllAsync()
        {
            return await _context.Matches
                .OrderByDescending(m => m.MatchDate)
                .ThenByDescending(m => m.MatchTime)
                .ToListAsync();
        }

        public async Task<List<NearbyMatchResult>> GetNearbyMatchesAsync(
    double latitude,
    double longitude,
    double radiusInKm)
        {
            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

            var matches = await _context.Matches
                .Where(m =>
                    m.Status == "Scheduled" ||
                    m.Status == "Live" ||
                    (m.Status == "Completed" && m.MatchDate >= sevenDaysAgo) ||
                    (m.Status == "Cancelled" && m.MatchDate >= sevenDaysAgo))
                .ToListAsync();

            return matches
                .Select(m => new NearbyMatchResult
                {
                    Match = m,
                    DistanceInKm = CalculateDistance(
                        latitude,
                        longitude,
                        m.Latitude,
                        m.Longitude)
                })
                .Where(x => x.DistanceInKm <= radiusInKm)
                .ToList();
        }

        private double CalculateDistance(double latitude1, double longitude1, double latitude2, double longitude2)
         {
            const double earthRadiusKm = 6371;

            var latitudeDifference =
                DegreesToRadians(latitude2 - latitude1);

            var longitudeDifference =
                DegreesToRadians(longitude2 - longitude1);

            var a =
                Math.Sin(latitudeDifference / 2) *
                Math.Sin(latitudeDifference / 2) +
                Math.Cos(DegreesToRadians(latitude1)) *
                Math.Cos(DegreesToRadians(latitude2)) *
                Math.Sin(longitudeDifference / 2) *
                Math.Sin(longitudeDifference / 2);

            var c = 2 * Math.Atan2(
                Math.Sqrt(a),
                Math.Sqrt(1 - a));

            return earthRadiusKm * c;
        }

        private double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }


        public async Task<List<Match>> GetMatchesByStateAsync(string state)
        {
            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

            return await _context.Matches
                .Where(m =>
                    m.State == state &&
                    (
                        m.Status == "Scheduled" ||
                        m.Status == "Live" ||
                        (m.Status == "Completed" && m.MatchDate >= sevenDaysAgo)
                    ))
                .OrderBy(m => m.MatchDate)
                .ThenBy(m => m.MatchTime)
                .ToListAsync();
        }

        public async Task<Match?> GetByIdForUpdateAsync(int matchId)
        {
            return await _context.Matches
                .FirstOrDefaultAsync(m => m.Id == matchId);
        }

        public async Task<Match?> UpdateAsync(Match match)
        {
            _context.Matches.Update(match);
            await _context.SaveChangesAsync();

            return match;
        }

        public async Task<bool> CancelAsync(int matchId)
        {
            var match = await _context.Matches
                .FirstOrDefaultAsync(m => m.Id == matchId);

            if (match == null)
            {
                return false;
            }

            match.Status = "Cancelled";
            match.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<Match>> GetMatchesByUmpireAsync(int umpireId)
        {
            return await _context.Matches
                .Where(m => m.UmpireId == umpireId)
                .OrderByDescending(m => m.MatchDate)
                .ThenByDescending(m => m.MatchTime)
                .ToListAsync();
        }
    }
}