namespace Rise.API.DTOs
{
    public class CreateEventRequest
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public required string Location { get; set; }
        public string? Theme { get; set; }
        public int? MaxParticipants { get; set; }
        public string? PosterUrl { get; set; }
        public string? DocumentUrl { get; set; }
        public List<string>? DocumentUrls { get; set; }
        public List<string>? ImageUrls { get; set; }
        public string? Rules { get; set; }
        
        // Nouveau : Lien vers le formulaire d'inscription
        public Guid? FormId { get; set; }
        public bool RequireFormSubmission { get; set; } = false;
    }

    public class UpdateEventRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Location { get; set; }
        public string? Theme { get; set; }
        public int? MaxParticipants { get; set; }
        public string? PosterUrl { get; set; }
        public string? DocumentUrl { get; set; }
        public string? Rules { get; set; }
        public List<string>? ImageUrls { get; set; }
        
        // Nouveau : Lien vers le formulaire d'inscription
        public Guid? FormId { get; set; }
        public bool? RequireFormSubmission { get; set; }
    }

    public class EventDTO
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public required string Location { get; set; }
        public string? Theme { get; set; }
        public int? MaxParticipants { get; set; }
        public int RegisteredCount { get; set; }
        public string? PosterUrl { get; set; }
        public string? DocumentUrl { get; set; }
        public string? Rules { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsPublished { get; set; }
        public bool IsUserRegistered { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>();
        
        // Nouveau : Formulaire d'inscription
        public Guid? FormId { get; set; }
        public bool RequireFormSubmission { get; set; }
        public FormDTO? Form { get; set; }
    }

    public class EventRegistrationRequest
    {
        public Guid EventId { get; set; }
        public Guid? FormSubmissionId { get; set; } // Optionnel si le formulaire est requis
    }

    public class EventRegistrationDTO
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public required string EventName { get; set; }
        public DateTime RegisteredAt { get; set; }
        public bool IsAttended { get; set; }
        public Guid? FormSubmissionId { get; set; }
    }
}
