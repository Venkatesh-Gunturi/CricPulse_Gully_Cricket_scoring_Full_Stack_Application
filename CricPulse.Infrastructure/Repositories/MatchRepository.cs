using CricPulse.Infrastructure.Data;
using CricPulse.Application.Interfaces.Match;
using CricPulse.Domain.Entities;
using CricPulse.Domain.Enums;
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
                .Include(m => m.MatchPlayers)
                    .ThenInclude(mp => mp.Player)
                        .ThenInclude(p => p.User)
                .Include(m => m.Innings)
                    .ThenInclude(i => i.Balls)
                        .ThenInclude(b => b.Wicket)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Match?> GetByIdForUpdateAsync(int id)
        {
            return await _context.Matches
                .Include(m => m.MatchPlayers)
                    .ThenInclude(mp => mp.Player)
                        .ThenInclude(p => p.User)
                .Include(m => m.Innings)
                    .ThenInclude(i => i.Balls)
                        .ThenInclude(b => b.Wicket)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<List<Match>> GetAllAsync()
        {
            return await _context.Matches
                .Include(m => m.MatchPlayers)
                    .ThenInclude(mp => mp.Player)
                        .ThenInclude(p => p.User)
                .Include(m => m.Innings)
                .ToListAsync();
        }

        public async Task<List<NearbyMatchResult>> GetNearbyMatchesAsync(
     double latitude,
     double longitude,
     double radiusInKm)
        {
            var matches = await _context.Matches
                .Include(m => m.MatchPlayers)
                    .ThenInclude(mp => mp.Player)
                        .ThenInclude(p => p.User)
                .Where(m =>
                    m.Status == MatchStatus.Scheduled ||
                    m.Status == MatchStatus.Live)
                .ToListAsync();

            return matches
                .Select(match =>
                {
                    var distance = CalculateDistanceInKm(
                        latitude,
                        longitude,
                        match.Latitude,
                        match.Longitude);

                    return new NearbyMatchResult
                    {
                        Match = match,
                        DistanceInKm = distance
                    };
                })
                .Where(x => x.DistanceInKm <= radiusInKm)
                .OrderBy(x => x.DistanceInKm)
                .ToList();
        }

        public async Task<List<Match>> GetMatchesByStateAsync(
            string state)
        {
            return await _context.Matches
                .Include(m => m.MatchPlayers)
                    .ThenInclude(mp => mp.Player)
                        .ThenInclude(p => p.User)
                .Where(m =>
                    m.State == state)
                .ToListAsync();
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

            match.Status = MatchStatus.Cancelled;
            match.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<Match>> GetMatchesByUmpireAsync(
            int umpireId)
        {
            return await _context.Matches
                .Include(m => m.MatchPlayers)
                    .ThenInclude(mp => mp.Player)
                        .ThenInclude(p => p.User)
                .Include(m => m.Innings)
                .Where(m =>
                    m.UmpireId == umpireId)
                .OrderByDescending(m =>
                    m.CreatedAt)
                .ToListAsync();
        }

        public async Task<Match?> GetLiveMatchAsync(
            int matchId)
        {
            return await _context.Matches

                .Include(m => m.MatchPlayers)
                    .ThenInclude(mp => mp.Player)
                        .ThenInclude(p => p.User)

                .Include(m => m.Innings)
                    .ThenInclude(i => i.StrikerMatchPlayer)
                        .ThenInclude(mp => mp.Player)
                            .ThenInclude(p => p.User)

                .Include(m => m.Innings)
                    .ThenInclude(i => i.NonStrikerMatchPlayer)
                        .ThenInclude(mp => mp.Player)
                            .ThenInclude(p => p.User)

                .Include(m => m.Innings)
                    .ThenInclude(i => i.CurrentBowlerMatchPlayer)
                        .ThenInclude(mp => mp.Player)
                            .ThenInclude(p => p.User)

                .Include(m => m.Innings)
                    .ThenInclude(i => i.Balls)
                        .ThenInclude(b => b.Wicket)
                            .ThenInclude(w => w.CaughtByMatchPlayer)
                                .ThenInclude(mp => mp.Player)
                                    .ThenInclude(p => p.User)

                .Include(m => m.Innings)
                    .ThenInclude(i => i.Balls)
                        .ThenInclude(b => b.Wicket)
                            .ThenInclude(w => w.StumpedByMatchPlayer)
                                .ThenInclude(mp => mp.Player)
                                    .ThenInclude(p => p.User)

                .FirstOrDefaultAsync(m =>
                    m.Id == matchId);
        }

        private static double CalculateDistanceInKm(
            double latitude1,
            double longitude1,
            double latitude2,
            double longitude2)
        {
            const double earthRadiusKm = 6371.0;

            var dLat =
                DegreesToRadians(
                    latitude2 - latitude1);

            var dLon =
                DegreesToRadians(
                    longitude2 - longitude1);

            var lat1 =
                DegreesToRadians(latitude1);

            var lat2 =
                DegreesToRadians(latitude2);

            var a =
                Math.Sin(dLat / 2) *
                Math.Sin(dLat / 2)
                +
                Math.Cos(lat1) *
                Math.Cos(lat2) *
                Math.Sin(dLon / 2) *
                Math.Sin(dLon / 2);

            var c =
                2 *
                Math.Atan2(
                    Math.Sqrt(a),
                    Math.Sqrt(1 - a));

            return earthRadiusKm * c;
        }

        private static double DegreesToRadians(
            double degrees)
        {
            return degrees *
                   Math.PI /
                   180.0;
        }
    }
}