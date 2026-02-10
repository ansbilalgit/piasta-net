namespace PiastaNet.API.Models
{
    public class Boardgame
    {
        public int Id { get; set; } // PK + FK to Item.Id

        public int MinPlayers { get; set; }
        public int MaxPlayers { get; set; }

        public int? BggId { get; set; } = -1;
        public double? BggRating { get; set; } = -1.0;
        public double? BggAverageRating { get; set; } = -1.0;
        public int? BggRank { get; set; } = -1;

        public int? LearnDifficulty { get; set; } = 0;
        public int? PlayDifficulty { get; set; } = 0;

        public Item Item { get; set; } = null!;
    }
}
