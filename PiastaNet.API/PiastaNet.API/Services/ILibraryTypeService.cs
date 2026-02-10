using PiastaNet.API.DTOs;
using PiastaNet.API.Models;

namespace PiastaNet.API.Services
{
    
    public interface ILibraryTypeService
    {
        Task<Item> CreateBoardgameAsync(BoardgameCreateDto dto, CancellationToken ct);
        Task<Item?> UpdateBoardgameAsync(int id, BoardgameUpdateDto dto, CancellationToken ct);

        Task<Item> CreateVideogameAsync(VideogameCreateDto dto, CancellationToken ct);
        Task<Item?> UpdateVideogameAsync(int id, VideogameUpdateDto dto, CancellationToken ct);
    }

}
