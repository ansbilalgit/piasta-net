namespace PiastaNet.API.Models
{
    public class GameEventParticipant
    {
        public Guid GameEventId { get; set; }
        public GameEvent GameEvent { get; set; }

        public string ParticipantUserId { get; set; }  // User being added
        public string RequestedByUserId { get; set; }  // Who did the action
    }
}
