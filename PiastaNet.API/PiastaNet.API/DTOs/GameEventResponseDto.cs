namespace PiastaNet.API.DTOs
{
    public class GameEventResponseDto
    {
        public Guid Id { get; set; }

        public int GameId { get; set; }
        public string GameName { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public int MinNumberOfPlayers { get; set; }
        public int MaxNumberOfPlayers { get; set; }

        public string OwnerUserId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
