namespace Rise.API.Models
{
    public class FormSubmission
    {
        public Guid Id { get; set; }
        public Guid FormId { get; set; }
        public Guid UserId { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        // Relationships
        public Form Form { get; set; } = null!;
        public User User { get; set; } = null!;
        public ICollection<Answer> Answers { get; set; } = new List<Answer>();
        public EventRegistration? EventRegistration { get; set; }
    }
}
