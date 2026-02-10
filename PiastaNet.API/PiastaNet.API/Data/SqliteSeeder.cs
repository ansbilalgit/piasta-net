using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PiastaNet.API.Models;

namespace PiastaNet.API.Data
{
    public static class SqliteSeeder
    {
        public static async Task SeedFromSqliteAsync(
            AppDbContext sqlServerDb,
            string sqlitePath,
            CancellationToken ct = default)
        {
            // Only seed if Azure SQL is empty
            if (await sqlServerDb.Items.AsNoTracking().AnyAsync(ct))
                return;

            if (!File.Exists(sqlitePath))
                throw new FileNotFoundException($"SQLite seed file not found: {sqlitePath}");

            var sqliteConnString = new SqliteConnectionStringBuilder
            {
                DataSource = sqlitePath,
                Mode = SqliteOpenMode.ReadOnly
            }.ToString();

            using var sqlite = new SqliteConnection(sqliteConnString);
            await sqlite.OpenAsync(ct);

            // Read games-only items
            var items = await ReadItemsAsync(sqlite, ct);
            var itemIds = items.Select(i => i.Id).ToHashSet();

            // Read related tables
            var categories = (await ReadCategoriesAsync(sqlite, ct))
                .Where(c => itemIds.Contains(c.ItemId))
                .ToList();

            var boardgames = (await ReadBoardgamesAsync(sqlite, ct))
                .Where(b => itemIds.Contains(b.Id))
                .ToList();

            var videogames = (await ReadVideogamesAsync(sqlite, ct))
                .Where(v => itemIds.Contains(v.Id))
                .ToList();

            // Attach categories
            var catsByItem = categories
                .GroupBy(c => c.ItemId)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var item in items)
            {
                if (catsByItem.TryGetValue(item.Id, out var cats))
                    item.Categories = cats;
            }

            // Attach 1-1 tables
            var bgById = boardgames.ToDictionary(x => x.Id, x => x);
            var vgById = videogames.ToDictionary(x => x.Id, x => x);

            foreach (var item in items)
            {
                if (item.Type == ItemType.Boardgame && bgById.TryGetValue(item.Id, out var bg))
                    item.Boardgame = bg;

                if (item.Type == ItemType.Videogame && vgById.TryGetValue(item.Id, out var vg))
                    item.Videogame = vg;
            }

            // Insert
            sqlServerDb.Items.AddRange(items);
            await sqlServerDb.SaveChangesAsync(ct);
        }

        private static async Task<List<Item>> ReadItemsAsync(SqliteConnection sqlite, CancellationToken ct)
        {
            var list = new List<Item>();

            using var cmd = sqlite.CreateCommand();
            cmd.CommandText = @"
SELECT id, name, length, description, thumbnail, type, copies
FROM items
WHERE type IN ('videogame','boardgame');";

            using var r = await cmd.ExecuteReaderAsync(ct);

            int ordId = r.GetOrdinal("id");
            int ordName = r.GetOrdinal("name");
            int ordLength = r.GetOrdinal("length");
            int ordDesc = r.GetOrdinal("description");
            int ordThumb = r.GetOrdinal("thumbnail");
            int ordType = r.GetOrdinal("type");
            int ordCopies = r.GetOrdinal("copies");

            while (await r.ReadAsync(ct))
            {
                var typeText = r.GetString(ordType);
                var type = typeText == "boardgame" ? ItemType.Boardgame : ItemType.Videogame;

                list.Add(new Item
                {
                    Id = r.GetInt32(ordId),
                    Name = r.GetString(ordName),
                    Length = r.IsDBNull(ordLength) ? null : r.GetInt32(ordLength),
                    Description = r.IsDBNull(ordDesc) ? "No description provided" : r.GetString(ordDesc),
                    Thumbnail = r.IsDBNull(ordThumb) ? null : r.GetString(ordThumb),
                    Type = type,
                    Copies = r.IsDBNull(ordCopies) ? null : r.GetInt32(ordCopies),
                });
            }

            return list;
        }

        private static async Task<List<Category>> ReadCategoriesAsync(SqliteConnection sqlite, CancellationToken ct)
        {
            var list = new List<Category>();

            using var cmd = sqlite.CreateCommand();
            cmd.CommandText = @"SELECT id, category FROM categories;";

            using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
            {
                list.Add(new Category
                {
                    ItemId = r.GetInt32(0),
                    Name = r.GetString(1)
                });
            }

            return list;
        }

        private static async Task<List<Boardgame>> ReadBoardgamesAsync(SqliteConnection sqlite, CancellationToken ct)
        {
            var list = new List<Boardgame>();

            using var cmd = sqlite.CreateCommand();
            cmd.CommandText = @"
SELECT id, min_players, max_players, bgg_id, bgg_rating, bgg_average_rating, bgg_rank, learn_difficulty, play_difficulty
FROM boardgames;";

            using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
            {
                list.Add(new Boardgame
                {
                    Id = r.GetInt32(0),
                    MinPlayers = r.GetInt32(1),
                    MaxPlayers = r.GetInt32(2),
                    BggId = r.IsDBNull(3) ? null : r.GetInt32(3),
                    BggRating = r.IsDBNull(4) ? null : r.GetDouble(4),
                    BggAverageRating = r.IsDBNull(5) ? null : r.GetDouble(5),
                    BggRank = r.IsDBNull(6) ? null : r.GetInt32(6),
                    LearnDifficulty = r.IsDBNull(7) ? null : r.GetInt32(7),
                    PlayDifficulty = r.IsDBNull(8) ? null : r.GetInt32(8)
                });
            }

            return list;
        }

        private static async Task<List<Videogame>> ReadVideogamesAsync(SqliteConnection sqlite, CancellationToken ct)
        {
            var list = new List<Videogame>();

            using var cmd = sqlite.CreateCommand();
            cmd.CommandText = @"
SELECT id, min_players, max_players, playing_time, difficulty, platform
FROM videogames;";

            using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
            {
                list.Add(new Videogame
                {
                    Id = r.GetInt32(0),
                    MinPlayers = r.GetInt32(1),
                    MaxPlayers = r.GetInt32(2),
                    PlayingTime = r.GetInt32(3),
                    Difficulty = r.IsDBNull(4) ? null : r.GetInt32(4),
                    Platform = r.IsDBNull(5) ? null : r.GetInt32(5)
                });
            }

            return list;
        }
    }
}
