namespace PiastaNet.API.DTOs
{
    public class CreateGameEventDto
    {
        public int GameId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public int? MinNumberOfPlayers { get; set; }
        public int? MaxNumberOfPlayers { get; set; }

        public string OwnerUserId { get; set; }
    }

    public record RegisterParticipantDto(
    Guid GameEventID,
    string ParticipantUserID,
    string RequestingUserID
);

}
