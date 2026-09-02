using CricPulse.Application.DTOs.Player;
using CricPulse.Application.Interfaces.player;
using CricPulse.Application.Interfaces.Player;
using PlayerEntity=CricPulse.Domain.Entities.Player;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CricPulse.Application.Services.Player
{
    public class PlayerService : IPlayerService
    {
        private readonly IPlayerRepository _playerRepository;

        public PlayerService(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        public async Task<PlayerResponseDto> CreateProfileAsync(int userId, CreatePlayerDto dto)
        {
            var existingPlayer = await _playerRepository.GetByUserIdAsync(userId);

            if (existingPlayer != null)
            {
                throw new InvalidOperationException(
                    "Player profile already exists.");
            }

            var player = new PlayerEntity
            {
                UserId = userId,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                BattingStyle = dto.BattingStyle,
                BowlingStyle = dto.BowlingStyle,
                PlayerRole = dto.PlayerRole,
                State = dto.State,
                PinCode = dto.PinCode
            };

            var createdPlayer = await _playerRepository.CreateAsync(player);

            return new PlayerResponseDto
            {
                Id = createdPlayer.Id,
                UserId = createdPlayer.UserId,
                DateOfBirth = createdPlayer.DateOfBirth,
                Gender = createdPlayer.Gender,
                BattingStyle = createdPlayer.BattingStyle,
                BowlingStyle = createdPlayer.BowlingStyle,
                PlayerRole = createdPlayer.PlayerRole,
                State = createdPlayer.State,
                PinCode = createdPlayer.PinCode
            };
        }

        public async Task<PlayerResponseDto?> GetProfileAsync(int userId)
        {
            var player = await _playerRepository.GetByUserIdAsync(userId);

            if (player == null)
            {
                return null;
            }

            return new PlayerResponseDto
            {
                Id = player.Id,
                UserId = player.UserId,
                DateOfBirth = player.DateOfBirth,
                Gender = player.Gender,
                BattingStyle = player.BattingStyle,
                BowlingStyle = player.BowlingStyle,
                PlayerRole = player.PlayerRole,
                State = player.State,
                PinCode = player.PinCode
            };
        }

        public async Task<PlayerResponseDto> UpdateProfileAsync(
    int userId,
    CreatePlayerDto dto)
        {
            var player = await _playerRepository.GetByUserIdAsync(userId);

            if (player == null)
            {
                throw new InvalidOperationException(
                    "Player profile not found.");
            }

            player.DateOfBirth = dto.DateOfBirth;
            player.Gender = dto.Gender;
            player.BattingStyle = dto.BattingStyle;
            player.BowlingStyle = dto.BowlingStyle;
            player.PlayerRole = dto.PlayerRole;
            player.State = dto.State;
            player.PinCode = dto.PinCode;

            var updatedPlayer = await _playerRepository.UpdateAsync(player);

            return new PlayerResponseDto
            {
                Id = updatedPlayer.Id,
                UserId = updatedPlayer.UserId,
                DateOfBirth = updatedPlayer.DateOfBirth,
                Gender = updatedPlayer.Gender,
                BattingStyle = updatedPlayer.BattingStyle,
                BowlingStyle = updatedPlayer.BowlingStyle,
                PlayerRole = updatedPlayer.PlayerRole,
                State = updatedPlayer.State,
                PinCode = updatedPlayer.PinCode
            };
        }
    }
}
