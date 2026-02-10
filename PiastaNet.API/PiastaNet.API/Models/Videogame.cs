namespace PiastaNet.API.Models
{
    public class Videogame
    {
        public int Id { get; set; } // PK + FK to Item.Id

        public int MinPlayers { get; set; }
        public int MaxPlayers { get; set; }

        public int PlayingTime { get; set; }

        public int? Difficulty { get; set; } = 0;
        public int? Platform { get; set; } = 0;

        public Item Item { get; set; } = null!;
    }
}
