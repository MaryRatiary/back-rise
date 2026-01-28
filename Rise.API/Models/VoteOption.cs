namespace Rise.API.Models
{
    public class VoteOption
    {
        public Guid Id { get; set; }
        public Guid PositionId { get; set; }
        public Guid? CandidateId { get; set; }
        public string? CandidateName { get; set; }
        public string? CandidateDescription { get; set; }
        public string? CandidateProfileUrl { get; set; }
        public DateTime SubmittedAt { get; set; }
        public bool IsApproved { get; set; } = false;

        // Relationships
        public VotePosition Position { get; set; } = null!;
        public User? Candidate { get; set; }
        public ICollection<VoteCast> VotesCast { get; set; } = new List<VoteCast>();
    }
}
