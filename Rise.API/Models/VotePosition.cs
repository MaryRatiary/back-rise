namespace Rise.API.Models
{
    public class VotePosition
    {
        public Guid Id { get; set; }
        public Guid VoteId { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }

        // Relationships
        public Vote Vote { get; set; } = null!;
        public ICollection<VoteOption> Options { get; set; } = new List<VoteOption>();
    }
}
