namespace Rise.API.Models
{
    public class PollOption
    {
        public Guid Id { get; set; }
        public Guid QuestionId { get; set; }
        public required string OptionText { get; set; }
        public int Order { get; set; }

        // Relationships
        public PollQuestion Question { get; set; } = null!;
        public ICollection<PollResponse> Responses { get; set; } = new List<PollResponse>();
    }
}
