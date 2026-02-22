using Microsoft.EntityFrameworkCore;
using PiastaNet.API.Data;
using PiastaNet.API.DTOs;
using PiastaNet.API.Models;

namespace PiastaNet.API.Services
{
    public class GameEventService : IGameEventService
    {
        private readonly AppDbContext _context;

        public GameEventService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<GameEventResponseDto> CreateAsync(CreateGameEventDto dto)
        {
            var game = await _context.Items
                .Include(i => i.Boardgame)
                .Include(i => i.Videogame)
                .FirstOrDefaultAsync(g => g.Id == dto.GameId);

            if (game == null)
                throw new Exception("Game not found");

            if (dto.StartTime >= dto.EndTime)
                throw new Exception("StartTime must be before EndTime");
            var gameMinPlayers = game.Type == ItemType.Boardgame ? game.Boardgame != null ? game.Boardgame.MinPlayers : null : (int?)(game.Videogame != null ? game.Videogame.MinPlayers : null);
            var gameMaxPlayers = game.Type == ItemType.Boardgame ? game.Boardgame != null ? game.Boardgame.MaxPlayers : null : (int?)(game.Videogame != null ? game.Videogame.MaxPlayers : null);
            var minPlayers = dto.MinNumberOfPlayers ?? gameMinPlayers ?? 1;
            var maxPlayers = dto.MaxNumberOfPlayers ?? gameMaxPlayers ?? 1;

            if ( minPlayers < gameMinPlayers)
                throw new Exception("Min players below allowed range");

            if ( maxPlayers > gameMaxPlayers)
                throw new Exception("Max players above allowed range");

            if (minPlayers > maxPlayers)
                throw new Exception("Min cannot exceed Max players");

            var entity = new GameEvent
            {
                Id = Guid.NewGuid(),
                GameId = dto.GameId,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                MinNumberOfPlayers = minPlayers,
                MaxNumberOfPlayers = maxPlayers,
                OwnerUserId = dto.OwnerUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.GameEvents.Add(entity);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(entity.Id);
        }

        public async Task<GameEventResponseDto> UpdateAsync(UpdateGameEventDto dto)
        {
            var entity = await _context.GameEvents
                .FirstOrDefaultAsync(e => e.Id == dto.GameEventId);

            if (entity == null)
                throw new Exception("Game event not found");

            if (entity.OwnerUserId != dto.OwnerUserId)
                throw new UnauthorizedAccessException("Only owner can update");

            if (dto.StartTime >= dto.EndTime)
                throw new Exception("StartTime must be before EndTime");

            var game = await _context.Items
                .FirstOrDefaultAsync(g => g.Id == dto.GameId);

            if (game == null)
                throw new Exception("Game not found");
            var gameMinPlayers = game.Type == ItemType.Boardgame ? game.Boardgame != null ? game.Boardgame.MinPlayers : null : (int?)(game.Videogame != null ? game.Videogame.MinPlayers : null);
            var gameMaxPlayers = game.Type == ItemType.Boardgame ? game.Boardgame != null ? game.Boardgame.MaxPlayers : null : (int?)(game.Videogame != null ? game.Videogame.MaxPlayers : null);
            var minPlayers = dto.MinNumberOfPlayers ?? gameMinPlayers ?? 1;
            var maxPlayers = dto.MaxNumberOfPlayers ?? gameMaxPlayers ?? 1;


            if (minPlayers < gameMinPlayers)
                throw new Exception("Min players below allowed range");

            if (maxPlayers > gameMaxPlayers)
                throw new Exception("Max players above allowed range");

            if (minPlayers > maxPlayers)
                throw new Exception("Min cannot exceed Max players");

            entity.GameId = dto.GameId;
            entity.StartTime = dto.StartTime;
            entity.EndTime = dto.EndTime;
            entity.MinNumberOfPlayers = minPlayers;
            entity.MaxNumberOfPlayers = maxPlayers;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(entity.Id);
        }

        public async Task DeleteAsync(Guid id, string ownerUserId)
        {
            var entity = await _context.GameEvents
                .FirstOrDefaultAsync(e => e.Id == id);

            if (entity == null)
                throw new Exception("Game event not found");

            if (entity.OwnerUserId != ownerUserId)
                throw new UnauthorizedAccessException("Only owner can delete");

            _context.GameEvents.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<GameEventResponseDto> GetByIdAsync(Guid id)
        {
            return await _context.GameEvents
                .Where(e => e.Id == id)
                .Select(e => new GameEventResponseDto
                {
                    Id = e.Id,
                    GameId = e.GameId,
                    GameName = e.Game.Name,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    MinNumberOfPlayers = e.MinNumberOfPlayers,
                    MaxNumberOfPlayers = e.MaxNumberOfPlayers,
                    OwnerUserId = e.OwnerUserId,
                    CreatedAt = e.CreatedAt
                })
                .FirstOrDefaultAsync();
        }
    }
}
