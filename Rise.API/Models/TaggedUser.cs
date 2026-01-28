namespace Rise.API.Models
{
    public class TaggedUser
    {
        public Guid Id { get; set; }
        public Guid CommentId { get; set; }
        public Guid TaggedUserId { get; set; }
        public DateTime CreatedAt { get; set; }

        // Relationships
        public Comment Comment { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
