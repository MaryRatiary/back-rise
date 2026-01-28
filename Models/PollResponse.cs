namespace Rise.API.Models
{
    public class PollResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid OptionId { get; set; }
        public DateTime RespondedAt { get; set; }

        // Relationships
        public User User { get; set; } = null!;
        public PollOption Option { get; set; } = null!;
    }
}
