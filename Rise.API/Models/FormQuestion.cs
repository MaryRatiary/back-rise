using Rise.API.Enums;

namespace Rise.API.Models
{
    public class FormQuestion
    {
        public Guid Id { get; set; }
        public Guid FormId { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public QuestionType Type { get; set; }
        public bool IsRequired { get; set; } = false;
        public int Order { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relationships
        public Form Form { get; set; } = null!;
        public ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
        public ICollection<Answer> Answers { get; set; } = new List<Answer>();
    }
}
