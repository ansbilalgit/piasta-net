using Microsoft.EntityFrameworkCore;
using PiastaNet.API.Data;
using PiastaNet.API.DTOs;
using PiastaNet.API.Models;

namespace PiastaNet.API.Services
{
    public class LibraryTypeService : ILibraryTypeService
    {
        private readonly AppDbContext _db;
        public LibraryTypeService(AppDbContext db) => _db = db;

        public async Task<Item> CreateBoardgameAsync(BoardgameCreateDto dto, CancellationToken ct)
        {
            await EnsureUniqueItemName(dto.Item.Name, ct);

            var item = new Item
            {
                // IMPORTANT: Id must be provided by client only if you want.
                // For normal CRUD, you can generate Id identity, BUT we chose ValueGeneratedNever for sqlite import.
                // So for new inserts, we must generate a new Id ourselves:
                Id = await NextItemId(ct),

                Name = dto.Item.Name,
                Length = dto.Item.Length,
                Description = dto.Item.Description ?? "No description provided",
                Thumbnail = dto.Item.Thumbnail,
                Copies = dto.Item.Copies ?? 1,
                Type = ItemType.Boardgame,
                Categories = ToCategories(dto.Item.Categories),
                Boardgame = new Boardgame
                {
                    // FK = item.Id (1:1)
                    Id = 0, // will set after item.Id assigned below
                    MinPlayers = dto.MinPlayers,
                    MaxPlayers = dto.MaxPlayers,
                    BggId = dto.BggId ?? -1,
                    BggRating = dto.BggRating ?? -1.0,
                    BggAverageRating = dto.BggAverageRating ?? -1.0,
                    BggRank = dto.BggRank ?? -1,
                    LearnDifficulty = dto.LearnDifficulty ?? 0,
                    PlayDifficulty = dto.PlayDifficulty ?? 0
                }
            };

            item.Boardgame.Id = item.Id;

            _db.Items.Add(item);
            await _db.SaveChangesAsync(ct);
            return item;
        }

        public async Task<Item?> UpdateBoardgameAsync(int id, BoardgameUpdateDto dto, CancellationToken ct)
        {
            var item = await _db.Items
                .Include(i => i.Categories)
                .Include(i => i.Boardgame)
                .FirstOrDefaultAsync(i => i.Id == id, ct);

            if (item is null) return null;
            if (item.Type != ItemType.Boardgame || item.Boardgame is null)
                throw new InvalidOperationException("Item is not a boardgame.");

            await EnsureUniqueItemName(dto.Item.Name, ct, exceptItemId: id);

            ApplyItemUpdate(item, dto.Item);
            ReplaceCategories(item, dto.Item.Categories);

            item.Boardgame.MinPlayers = dto.MinPlayers;
            item.Boardgame.MaxPlayers = dto.MaxPlayers;
            item.Boardgame.BggId = dto.BggId ?? item.Boardgame.BggId;
            item.Boardgame.BggRating = dto.BggRating ?? item.Boardgame.BggRating;
            item.Boardgame.BggAverageRating = dto.BggAverageRating ?? item.Boardgame.BggAverageRating;
            item.Boardgame.BggRank = dto.BggRank ?? item.Boardgame.BggRank;
            item.Boardgame.LearnDifficulty = dto.LearnDifficulty ?? item.Boardgame.LearnDifficulty;
            item.Boardgame.PlayDifficulty = dto.PlayDifficulty ?? item.Boardgame.PlayDifficulty;

            await _db.SaveChangesAsync(ct);
            return item;
        }

        public async Task<Item> CreateVideogameAsync(VideogameCreateDto dto, CancellationToken ct)
        {
            await EnsureUniqueItemName(dto.Item.Name, ct);

            var item = new Item
            {
                Id = await NextItemId(ct),
                Name = dto.Item.Name,
                Length = dto.Item.Length,
                Description = dto.Item.Description ?? "No description provided",
                Thumbnail = dto.Item.Thumbnail,
                Copies = dto.Item.Copies ?? 1,
                Type = ItemType.Videogame,
                Categories = ToCategories(dto.Item.Categories),
                Videogame = new Videogame
                {
                    Id = 0, // set below
                    MinPlayers = dto.MinPlayers,
                    MaxPlayers = dto.MaxPlayers,
                    PlayingTime = dto.PlayingTime,
                    Difficulty = dto.Difficulty ?? 0,
                    Platform = dto.Platform ?? 0
                }
            };

            item.Videogame.Id = item.Id;

            _db.Items.Add(item);
            await _db.SaveChangesAsync(ct);
            return item;
        }

        public async Task<Item?> UpdateVideogameAsync(int id, VideogameUpdateDto dto, CancellationToken ct)
        {
            var item = await _db.Items
                .Include(i => i.Categories)
                .Include(i => i.Videogame)
                .FirstOrDefaultAsync(i => i.Id == id, ct);

            if (item is null) return null;
            if (item.Type != ItemType.Videogame || item.Videogame is null)
                throw new InvalidOperationException("Item is not a videogame.");

            await EnsureUniqueItemName(dto.Item.Name, ct, exceptItemId: id);

            ApplyItemUpdate(item, dto.Item);
            ReplaceCategories(item, dto.Item.Categories);

            item.Videogame.MinPlayers = dto.MinPlayers;
            item.Videogame.MaxPlayers = dto.MaxPlayers;
            item.Videogame.PlayingTime = dto.PlayingTime;
            item.Videogame.Difficulty = dto.Difficulty ?? item.Videogame.Difficulty;
            item.Videogame.Platform = dto.Platform ?? item.Videogame.Platform;

            await _db.SaveChangesAsync(ct);
            return item;
        }

        private async Task EnsureUniqueItemName(string name, CancellationToken ct, int? exceptItemId = null)
        {
            var exists = await _db.Items.AnyAsync(
                i => i.Name == name && (exceptItemId == null || i.Id != exceptItemId),
                ct);

            if (exists) throw new InvalidOperationException("Item name must be unique.");
        }

        private async Task<int> NextItemId(CancellationToken ct)
        {
            // Because Id is ValueGeneratedNever, we generate ids manually for new inserts
            var max = await _db.Items.MaxAsync(i => (int?)i.Id, ct);
            return (max ?? 0) + 1;
        }

        private static List<Category> ToCategories(List<string>? categories)
            => (categories ?? new List<string>())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(s => new Category { Name = s.Trim() })
                .ToList();

        private static void ReplaceCategories(Item item, List<string>? categories)
        {
            item.Categories.Clear();
            foreach (var c in ToCategories(categories))
                item.Categories.Add(new Category { ItemId = item.Id, Name = c.Name });
        }

        private static void ApplyItemUpdate(Item item, ItemBaseUpdateDto dto)
        {
            item.Name = dto.Name;
            item.Length = dto.Length;
            item.Description = dto.Description ?? item.Description;
            item.Thumbnail = dto.Thumbnail ?? item.Thumbnail;
            item.Copies = dto.Copies ?? item.Copies;
        }
    }

}
