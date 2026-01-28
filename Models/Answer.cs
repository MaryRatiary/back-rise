namespace Rise.API.Models
{
    public class Answer
    {
        public Guid Id { get; set; }
        public Guid SubmissionId { get; set; }
        public Guid QuestionId { get; set; }
        public Guid? OptionId { get; set; } // Pour les questions avec options
        public string? ResponseValue { get; set; } // Pour les textes, nombres, emails, etc.
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relationships
        public FormSubmission Submission { get; set; } = null!;
        public FormQuestion Question { get; set; } = null!;
        public QuestionOption? Option { get; set; }
        public ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();
    }
}
