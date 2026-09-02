using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PlayerEntity= CricPulse.Domain.Entities.Player;

namespace CricPulse.Application.Interfaces.player
{
    public interface IPlayerRepository
    {
        Task<PlayerEntity?> GetByUserIdAsync(int userId);

        Task<PlayerEntity> CreateAsync(PlayerEntity player);

        Task<PlayerEntity> UpdateAsync(PlayerEntity player);
    }
}
