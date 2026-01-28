namespace Rise.API.Models
{
    public class Post
    {
        public Guid Id { get; set; }
        public Guid? EventId { get; set; }
        public Guid CreatedBy { get; set; }
        public required string Content { get; set; }
        public string? VideoUrl { get; set; }
        public string? ExternalLink { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int ReactionCount { get; set; } = 0;
        public int CommentCount { get; set; } = 0;
        public bool IsPublic { get; set; } = true;
        
        // Nouveaux champs pour stocker les IDs des utilisateurs qui ont aimé
        public List<Guid> LikedByUserIds { get; set; } = new List<Guid>();
        
        // Champ JSON pour stocker les likes avec détails utilisateur
        public string? LikesData { get; set; } // JSON string: [{"userId": "...", "userName": "...", "profileImage": "..."}]

        // Relationships
        public Event? Event { get; set; }
        public ICollection<PostImage> Images { get; set; } = new List<PostImage>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
    }

    public class PostImage
    {
        public Guid Id { get; set; }
        public Guid PostId { get; set; }
        public required string ImageUrl { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }

        // Relationships
        public Post Post { get; set; } = null!;
    }
}
