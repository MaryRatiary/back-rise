namespace Rise.API.Models
{
    public class VoteCast
    {
        public Guid Id { get; set; }
        public Guid VoteId { get; set; }
        public Guid OptionId { get; set; }
        public Guid UserId { get; set; }
        public DateTime CastAt { get; set; }

        // Relationships
        public Vote Vote { get; set; } = null!;
        public VoteOption Option { get; set; } = null!;
    }
}
