namespace Rise.API.Models
{
    public class PollQuestion
    {
        public Guid Id { get; set; }
        public Guid PollId { get; set; }
        public required string QuestionText { get; set; }
        public bool AllowMultipleChoice { get; set; } = false;
        public int Order { get; set; }

        // Relationships
        public Poll Poll { get; set; } = null!;
        public ICollection<PollOption> Options { get; set; } = new List<PollOption>();
    }
}
