namespace Rise.API.Models
{
    public class Poll
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string TargetAudience { get; set; } = "all"; // all, students, professors
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public bool IsActive { get; set; } = true;

        // Relationships
        public ICollection<PollQuestion> Questions { get; set; } = new List<PollQuestion>();
    }
}
