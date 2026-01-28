using Rise.API.Enums;

namespace Rise.API.Models
{
    public class Event
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required EventType Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public required string Location { get; set; }
        public string? Theme { get; set; }
        public int? MaxParticipants { get; set; }
        public string? PosterUrl { get; set; }
        public string? DocumentUrl { get; set; }
        public string? Rules { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsPublished { get; set; } = false;
        public Guid CreatedBy { get; set; }
        
        // Nouveau : Lien vers le formulaire d'inscription
        public Guid? FormId { get; set; }
        public bool RequireFormSubmission { get; set; } = false;

        // Relationships
        public ICollection<EventRegistration> Registrations { get; set; } = new List<EventRegistration>();
        public ICollection<User> Juries { get; set; } = new List<User>();
        public ICollection<Post> Posts { get; set; } = new List<Post>();
        public Form? Form { get; set; }
    }
}
