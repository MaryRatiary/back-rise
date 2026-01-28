namespace Rise.API.Models
{
    public class Vote
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public bool IsActive { get; set; } = false;
        public bool ResultsPublished { get; set; } = false;

        // Relationships
        public ICollection<VotePosition> Positions { get; set; } = new List<VotePosition>();
        public ICollection<VoteCast> VotesCast { get; set; } = new List<VoteCast>();
    }
}
