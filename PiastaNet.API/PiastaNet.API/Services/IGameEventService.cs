using PiastaNet.API.DTOs;

namespace PiastaNet.API.Services
{
    public interface IGameEventService
    {
        Task<GameEventResponseDto> CreateAsync(CreateGameEventDto dto);
        Task<GameEventResponseDto> UpdateAsync(UpdateGameEventDto dto);
        Task DeleteAsync(Guid id, string ownerUserId);
        Task<GameEventResponseDto> GetByIdAsync(Guid id);
    }
}
