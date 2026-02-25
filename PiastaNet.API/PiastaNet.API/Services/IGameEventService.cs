using PiastaNet.API.DTOs;

namespace PiastaNet.API.Services
{
    public interface IGameEventService
    {
        Task<GameEventResponseDto> CreateAsync(CreateGameEventDto dto);
        Task<GameEventResponseDto> UpdateAsync(UpdateGameEventDto dto);
        Task DeleteAsync(Guid id, string ownerUserId);
        Task<GameEventResponseDto> GetByIdAsync(Guid id);
        Task<PagedResult<GameEventResponseDto>> GetAllAsync(
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
    CancellationToken ct);

        Task AddParticipantAsync(RegisterParticipantDto dto);
        Task RemoveParticipantAsync(RegisterParticipantDto dto);
    }
}
