using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CricPulse.Application.Interfaces.player;
using CricPulse.Domain.Entities;
using CricPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CricPulse.Infrastructure.Repositories
{
    public class PlayerRepository :IPlayerRepository
    {
        private readonly CricPulseDbContext _context;

        public PlayerRepository(CricPulseDbContext context)
        {
            _context = context;
        }

        public async Task<Player?> GetByUserIdAsync(int userId)
        {
            return await _context.Players
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task<Player> CreateAsync(Player player)
        {
            await _context.Players.AddAsync(player);
            await _context.SaveChangesAsync();

            return player;
        }

        public async Task<Player> UpdateAsync(Player player)
        {
            _context.Players.Update(player);
            await _context.SaveChangesAsync();

            return player;
        }
    }
}
