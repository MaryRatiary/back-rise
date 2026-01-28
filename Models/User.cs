using Rise.API.Enums;

namespace Rise.API.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public required string MatriculeNumber { get; set; } // Numéro de matricule unique et obligatoire
        public string? Filiere { get; set; } // Génie Logiciel, Intelligence Artificielle, Administration Réseau et Système (null pour L1)
        public required string Classe { get; set; } // L1, L2, L3, M1, M2
        public required UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public string? ProfileImageUrl { get; set; }
        public string? CoverImageUrl { get; set; }

        // Informations professionnelles et personnelles
        public string? JobTitle { get; set; }
        public string? Company { get; set; }
        public string? Location { get; set; }
        public string? Phone { get; set; }
        public string? Bio { get; set; }

        // Informations académiques
        public string? Specialization { get; set; } // Spécialisation ou option choisie

        // Centres d'intérêt
        public string? InterestCategories { get; set; } // JSON: ["sport", "culture", "tech", "associatif"]
        public string? Associations { get; set; } // JSON: liste des associations
        public string? SharedExpertise { get; set; } // JSON: liste des compétences

        // Langues
        public string? Languages { get; set; } // JSON: ["Français", "Anglais", "Espagnol"]

        // Réseaux sociaux
        public string? LinkedinUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? TwitterUrl { get; set; }
        public string? GithubUrl { get; set; }

        // QR Code
        public string? QrCodeUrl { get; set; }

        // Préférences
        public string? NotificationPreferences { get; set; } // JSON: quels types d'événements intéressent
        public string? ProfileVisibility { get; set; } // "public", "friends", "private"

        // Statistiques
        public int EventsJoined { get; set; } = 0;
        public int EventsAttended { get; set; } = 0;
        public string? Badges { get; set; } // JSON: liste des badges obtenus

        // Relationships
        public ICollection<EventRegistration> EventRegistrations { get; set; } = new List<EventRegistration>();
        public ICollection<PollResponse> PollResponses { get; set; } = new List<PollResponse>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
        public ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
        public ICollection<Conversation> Conversations1 { get; set; } = new List<Conversation>();
        public ICollection<Conversation> Conversations2 { get; set; } = new List<Conversation>();
        public ICollection<MessageReaction> MessageReactions { get; set; } = new List<MessageReaction>();
        public ICollection<VoteOption> Candidates { get; set; } = new List<VoteOption>();
        public ICollection<TaggedUser> TaggedInComments { get; set; } = new List<TaggedUser>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<Notification> TriggeredNotifications { get; set; } = new List<Notification>();
        public ICollection<FormSubmission> FormSubmissions { get; set; } = new List<FormSubmission>();
        public ICollection<Form> CreatedForms { get; set; } = new List<Form>();
    }
}
