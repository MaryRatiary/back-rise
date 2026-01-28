namespace Rise.API.Models
{
    public class EventRegistration
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public Guid UserId { get; set; }
        public DateTime RegisteredAt { get; set; }
        public bool IsAttended { get; set; } = false;
        
        // Nouveau : Lien vers la soumission du formulaire
        public Guid? FormSubmissionId { get; set; }

        // Relationships
        public Event Event { get; set; } = null!;
        public User User { get; set; } = null!;
        public FormSubmission? FormSubmission { get; set; }
    }
}
