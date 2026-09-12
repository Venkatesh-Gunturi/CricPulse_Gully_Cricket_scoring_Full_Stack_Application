using CricPulse.Application.Interfaces.Match;
using CricPulse.Domain.Enums;
using CricPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using MatchEntity=CricPulse.Domain.Entities.Match;

namespace CricPulse.Infrastructure.Repositories
{
    public class MatchRepository : IMatchRepository
    {
        private readonly CricPulseDbContext _context;

        public MatchRepository(CricPulseDbContext context)
        {
            _context = context;
        }

        public async Task<MatchEntity> CreateAsync(MatchEntity match)
        {
            await _context.Matches.AddAsync(match);
            await _context.SaveChangesAsync();

            return match;
        }

        public async Task<MatchEntity?> GetByIdAsync(int id)
        {
            return await _context.Matches
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        //All matches are fetching to display on Home page
        public async Task<List<MatchEntity>> GetAllAsync()
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
                    m.Status == MatchStatus.Scheduled ||
                    m.Status == MatchStatus.Live ||
                    (m.Status == MatchStatus.Completed &&
                     m.MatchDate >= sevenDaysAgo) ||
                    (m.Status == MatchStatus.Cancelled &&
                     m.MatchDate >= sevenDaysAgo))
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


        public async Task<List<MatchEntity>> GetMatchesByStateAsync(string state)
        {
            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

            return await _context.Matches
                .Where(m =>
                    m.State == state &&
                    (
                        m.Status == MatchStatus.Scheduled ||
                        m.Status == MatchStatus.Live ||
                        (m.Status == MatchStatus.Completed &&
                         m.MatchDate >= sevenDaysAgo)
                    ))
                .OrderBy(m => m.MatchDate)
                .ThenBy(m => m.MatchTime)
                .ToListAsync();
        }

        public async Task<MatchEntity?> GetByIdForUpdateAsync(int matchId)
        {
            return await _context.Matches
                .FirstOrDefaultAsync(m => m.Id == matchId);
        }

        public async Task<MatchEntity?> UpdateAsync(MatchEntity match)
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

            match.Status = MatchStatus.Cancelled;
            match.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<MatchEntity>> GetMatchesByUmpireAsync(int umpireId)
        {
            return await _context.Matches
                .Where(m => m.UmpireId == umpireId)
                .OrderByDescending(m => m.MatchDate)
                .ThenByDescending(m => m.MatchTime)
                .ToListAsync();
        }

        // Purpose:
        // Load the match with its players, toss information, innings, balls,
        // and wicket details for the live match screen.
        public async Task<MatchEntity?> GetLiveMatchAsync(int matchId)
        {
            return await _context.Matches
                .Include(m => m.MatchPlayers)
                    .ThenInclude(mp => mp.Player)
                .Include(m => m.Innings)
                    .ThenInclude(i => i.Balls)
                        .ThenInclude(b => b.Wicket)
                .FirstOrDefaultAsync(m => m.Id == matchId);
        }
    }
}