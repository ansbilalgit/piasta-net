using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PiastaNet.API.Data;
using PiastaNet.API.DTOs;
using PiastaNet.API.Models;
using PiastaNet.API.Services;

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
                .Include(e => e.Participants)
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
                    CreatedAt = e.CreatedAt,
                    Participants = e.Participants.Select(p => p.ParticipantUserId).Distinct().ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<PagedResult<GameEventResponseDto>> GetAllAsync(
    string? q,
    int? gameId,
    string? ownerUserId,
    DateTime? from,
    DateTime? to,
    bool? upcomingOnly,
    bool? pastOnly,
    int? minPlayers,
    int? maxPlayers,
    bool? hasAvailableSlots,
    string? sortBy,
    string? sortDir,
    int page,
    int pageSize,
    CancellationToken ct)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 20 : pageSize;
            pageSize = Math.Min(pageSize, 100);

            IQueryable<GameEvent> query = _context.GameEvents
                .Include(e => e.Game)
                //.Include(e => e.Participants) // if you track participants
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var s = q.Trim();
                query = query.Where(e => e.Game.Name.Contains(s));
            }

            if (gameId.HasValue)
                query = query.Where(e => e.GameId == gameId.Value);

            if (!string.IsNullOrWhiteSpace(ownerUserId))
                query = query.Where(e => e.OwnerUserId == ownerUserId);

            if (from.HasValue)
                query = query.Where(e => e.StartTime >= from.Value);

            if (to.HasValue)
                query = query.Where(e => e.EndTime <= to.Value);

            if (upcomingOnly == true)
                query = query.Where(e => e.StartTime > DateTime.UtcNow);

            if (pastOnly == true)
                query = query.Where(e => e.EndTime < DateTime.UtcNow);

            if (minPlayers.HasValue)
                query = query.Where(e => e.MinNumberOfPlayers >= minPlayers.Value);

            if (maxPlayers.HasValue)
                query = query.Where(e => e.MaxNumberOfPlayers <= maxPlayers.Value);

            
            //if (hasAvailableSlots == true)
            //{
            //    query = query.Where(e =>
            //        e.Participants.Count < e.MaxNumberOfPlayers);
            //}

            // 🔃 Sorting
            var desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);

            query = (sortBy?.Trim().ToLowerInvariant()) switch
            {
                "starttime" => desc
                    ? query.OrderByDescending(e => e.StartTime)
                    : query.OrderBy(e => e.StartTime),

                "endtime" => desc
                    ? query.OrderByDescending(e => e.EndTime)
                    : query.OrderBy(e => e.EndTime),

                "game" => desc
                    ? query.OrderByDescending(e => e.Game.Name)
                    : query.OrderBy(e => e.Game.Name),

                "minplayers" => desc
                    ? query.OrderByDescending(e => e.MinNumberOfPlayers)
                    : query.OrderBy(e => e.MinNumberOfPlayers),

                "maxplayers" => desc
                    ? query.OrderByDescending(e => e.MaxNumberOfPlayers)
                    : query.OrderBy(e => e.MaxNumberOfPlayers),

                _ => desc
                    ? query.OrderByDescending(e => e.Id)
                    : query.OrderBy(e => e.Id)
            };

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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
                    CreatedAt = e.CreatedAt,
                    Participants = e.Participants.Select(p => p.ParticipantUserId).Distinct().ToList()
                })
                .ToListAsync(ct);

            return new PagedResult<GameEventResponseDto>(page, pageSize, totalCount, items);
        }
    
    
    public async Task AddParticipantAsync(RegisterParticipantDto dto)
        {

            var gameEvent = await _context.GameEvents
                .Include(e => e.Participants)
                .FirstOrDefaultAsync(e => e.Id == dto.GameEventID);

            if (gameEvent == null)
                throw new Exception("GameEvent not found.");

            // Only owner or self
            if (dto.RequestingUserID != gameEvent.OwnerUserId &&
                dto.RequestingUserID != dto.ParticipantUserID)
                throw new UnauthorizedAccessException();

            // Max player check
            var uniqueCount = gameEvent.Participants.Select(p => p.ParticipantUserId).Distinct().Count();
            if (uniqueCount >= gameEvent.MaxNumberOfPlayers)
                throw new Exception("Max players reached.");

            // Prevent duplicate participant-requestedBy pair
            if (!gameEvent.Participants.Any(p =>
                    p.ParticipantUserId == dto.ParticipantUserID &&
                    p.RequestedByUserId == dto.RequestingUserID))
            {
                gameEvent.Participants.Add(new GameEventParticipant
                {
                    GameEventId = gameEvent.Id,
                    ParticipantUserId = dto.ParticipantUserID,
                    RequestedByUserId = dto.RequestingUserID
                });

                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveParticipantAsync(RegisterParticipantDto dto)
        {

            var gameEvent = await _context.GameEvents
                .Include(e => e.Participants)
                .FirstOrDefaultAsync(e => e.Id == dto.GameEventID);

            if (gameEvent == null)
                throw new Exception("GameEvent not found.");

            // Only owner or self
            if (dto.RequestingUserID != gameEvent.OwnerUserId &&
                dto.RequestingUserID != dto.ParticipantUserID)
                throw new UnauthorizedAccessException();

            var participant = gameEvent.Participants
                .FirstOrDefault(p => p.ParticipantUserId == dto.ParticipantUserID);

            if (participant != null)
            {
                gameEvent.Participants.Remove(participant);
                await _context.SaveChangesAsync();
            }
        }
    }
}
