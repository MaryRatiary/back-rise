namespace Rise.API.Models
{
    public class Form
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsPublished { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public string TargetAudience { get; set; } = "all"; // all, students, professors
        public bool AllowMultipleResponses { get; set; } = false;

        // Relationships
        public User? CreatedByUser { get; set; }
        public ICollection<FormQuestion> Questions { get; set; } = new List<FormQuestion>();
        public ICollection<FormSubmission> Submissions { get; set; } = new List<FormSubmission>();
    }
}
