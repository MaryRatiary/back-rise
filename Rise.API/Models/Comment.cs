namespace Rise.API.Models
{
    public class Comment
    {
        public Guid Id { get; set; }
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }
        public Guid? ParentCommentId { get; set; } // Pour les réponses aux commentaires
        public required string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int ReactionCount { get; set; } = 0;

        // Relationships
        public Post Post { get; set; } = null!;
        public User User { get; set; } = null!;
        public Comment? ParentComment { get; set; }
        public ICollection<Comment> Replies { get; set; } = new List<Comment>();
        public ICollection<TaggedUser> TaggedUsers { get; set; } = new List<TaggedUser>();
        public ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
    }
}
