namespace Rise.API.Models
{
    public class Message
    {
        public Guid Id { get; set; }
        public Guid ConversationId { get; set; }
        public Guid SenderId { get; set; }
        public required string Content { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; } = false;
        
        // Reply to message
        public Guid? ReplyToId { get; set; }

        // Relationships
        public User Sender { get; set; } = null!;
        public Conversation Conversation { get; set; } = null!;
        public Message? ReplyTo { get; set; }
        public ICollection<MessageReaction> Reactions { get; set; } = new List<MessageReaction>();
    }

    public class Conversation
    {
        public Guid Id { get; set; }
        public Guid UserId1 { get; set; }
        public Guid UserId2 { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

        // Relationships
        public User User1 { get; set; } = null!;
        public User User2 { get; set; } = null!;
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }

    public class MessageReaction
    {
        public Guid Id { get; set; }
        public Guid MessageId { get; set; }
        public Guid UserId { get; set; }
        public required string Emoji { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relationships
        public Message Message { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
