namespace Rise.API.Models
{
    public class QuestionOption
    {
        public Guid Id { get; set; }
        public Guid QuestionId { get; set; }
        public required string OptionText { get; set; }
        public int Order { get; set; }

        // Relationships
        public FormQuestion Question { get; set; } = null!;
        public ICollection<Answer> Answers { get; set; } = new List<Answer>();
    }
}
