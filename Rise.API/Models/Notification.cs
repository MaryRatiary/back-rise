namespace Rise.API.Models
{
    public class Notification
    {
        public Guid Id { get; set; }
        public Guid RecipientId { get; set; }
        public Guid TriggeredByUserId { get; set; }
        public required string Type { get; set; } // "comment", "reaction", "mention", "reply"
        public Guid? PostId { get; set; }
        public Guid? CommentId { get; set; }
        public required string Message { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; }

        // Relationships
        public User Recipient { get; set; } = null!;
        public User TriggeredByUser { get; set; } = null!;
        public Post? Post { get; set; }
        public Comment? Comment { get; set; }
    }
}
