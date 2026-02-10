using Microsoft.EntityFrameworkCore;
using PiastaNet.API.Data;
using PiastaNet.API.DTOs;
using PiastaNet.API.Models;
using System.Linq;

namespace PiastaNet.API.Services
{
    public class ItemsService : IItemsService
    {
        private readonly AppDbContext _db;

        public ItemsService(AppDbContext db) => _db = db;


        public async Task<PagedResult<ItemListDto>> GetAllAsync(
            string? q,
            string? type,
            string? category,
            string? sortBy,
            string? sortDir,
            int page,
            int pageSize,
            CancellationToken ct)
        {
            // Safety limits
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 20 : pageSize;
            pageSize = Math.Min(pageSize, 100);

            IQueryable<Item> query = _db.Items
                .Include(i => i.Categories)
                .AsNoTracking();

            // Filter: search
            if (!string.IsNullOrWhiteSpace(q))
            {
                var s = q.Trim();
                query = query.Where(i =>
                    i.Name.Contains(s) ||
                    i.Description.Contains(s));
            }

            // Filter: type (boardgame/videogame)
            if (!string.IsNullOrWhiteSpace(type))
            {
                var t = type.Trim().ToLowerInvariant();
                query = t switch
                {
                    "boardgame" => query.Where(i => i.Type == ItemType.Boardgame),
                    "videogame" => query.Where(i => i.Type == ItemType.Videogame),
                    _ => query // ignore invalid type
                };
            }

            // Filter: category
            if (!string.IsNullOrWhiteSpace(category))
            {
                var c = category.Trim();
                query = query.Where(i => i.Categories.Any(x => x.Name == c));
            }

            // Sorting
            var desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);

            query = (sortBy?.Trim().ToLowerInvariant()) switch
            {
                "name" => desc ? query.OrderByDescending(i => i.Name) : query.OrderBy(i => i.Name),
                "type" => desc ? query.OrderByDescending(i => i.Type) : query.OrderBy(i => i.Type),
                "copies" => desc ? query.OrderByDescending(i => i.Copies) : query.OrderBy(i => i.Copies),
                "length" => desc ? query.OrderByDescending(i => i.Length) : query.OrderBy(i => i.Length),
                "id" or null or "" => desc ? query.OrderByDescending(i => i.Id) : query.OrderBy(i => i.Id),
                _ => desc ? query.OrderByDescending(i => i.Id) : query.OrderBy(i => i.Id)
            };

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(i => new ItemListDto(
                    i.Id,
                    i.Name,
                    i.Length,
                    i.Description,
                    i.Thumbnail,
                    i.Type,
                    i.Copies,
                    i.Categories.Select(c => c.Name).ToList()
                ))
                .ToListAsync(ct);

            return new PagedResult<ItemListDto>(page, pageSize, totalCount, items);
        }

        public Task<Item?> GetByIdAsync(int id, CancellationToken ct)
            => _db.Items
                .Include(i => i.Categories)
                .Include(i => i.Boardgame)
                .Include(i => i.Videogame)
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == id, ct);

        public async Task<bool> DeleteAsync(int id, CancellationToken ct)
        {
            var item = await _db.Items.FirstOrDefaultAsync(i => i.Id == id, ct);
            if (item is null) return false;

            _db.Items.Remove(item);
            await _db.SaveChangesAsync(ct);
            return true;
        }

    }

}
