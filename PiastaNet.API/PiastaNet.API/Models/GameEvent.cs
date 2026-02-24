namespace PiastaNet.API.Models
{
    public class GameEvent
    {
        public Guid Id { get; set; }

        public int GameId { get; set; }   // FK to Item table
        public Item Game { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public int MinNumberOfPlayers { get; set; }
        public int MaxNumberOfPlayers { get; set; }

        public string OwnerUserId { get; set; }

        public DateTime CreatedAt { get; set; }
        public ICollection<GameEventParticipant> Participants { get; set; } = new List<GameEventParticipant>();

    }
}
