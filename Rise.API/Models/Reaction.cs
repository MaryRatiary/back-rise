namespace Rise.API.Models
{
    public class Reaction
    {
        public Guid Id { get; set; }
        public Guid? PostId { get; set; }
        public Guid? CommentId { get; set; }
        public Guid UserId { get; set; }
        public required string EmojiType { get; set; } // like, love, haha, wow, sad, angry
        public DateTime CreatedAt { get; set; }

        // Relationships
        public Post? Post { get; set; }
        public Comment? Comment { get; set; }
        public User User { get; set; } = null!;
    }
}
