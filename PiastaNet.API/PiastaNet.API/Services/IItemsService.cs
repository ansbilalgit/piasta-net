using PiastaNet.API.DTOs;
using PiastaNet.API.Models;

namespace PiastaNet.API.Services
{

    public interface IItemsService
    {
        Task<PagedResult<ItemListDto>> GetAllAsync(
        string? q,
        string? type,
        string? category,
        string? sortBy,
        string? sortDir,
        int page,
        int pageSize,
        CancellationToken ct);
        Task<ItemListDto?> GetByIdAsync(int id, CancellationToken ct);
        Task<bool> DeleteAsync(int id, CancellationToken ct);
    }
}
