namespace Rise.API.DTOs
{
    public class MessageDto
    {
        public Guid Id { get; set; }
        public Guid ConversationId { get; set; }
        public Guid SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string SenderProfileImageUrl { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        public bool IsOwn { get; set; }
        
        // Reply to
        public MessageReplyDto? ReplyTo { get; set; }
        
        // Reactions
        public List<MessageReactionDto> Reactions { get; set; } = new();
    }

    public class MessageReplyDto
    {
        public Guid Id { get; set; }
        public string Sender { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    public class MessageReactionDto
    {
        public Guid Id { get; set; }
        public string Emoji { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class ConversationDto
    {
        public Guid Id { get; set; }
        public string Sender { get; set; } = string.Empty;
        public string SenderProfileImageUrl { get; set; } = string.Empty;
        public string LastMessage { get; set; } = string.Empty;
        public DateTime Time { get; set; }
        public int Unread { get; set; }
        public List<MessageDto> Messages { get; set; } = new();
    }

    public class CreateMessageRequest
    {
        public Guid ConversationId { get; set; }
        public string Content { get; set; } = string.Empty;
        public Guid? ReplyToId { get; set; }
    }

    public class StartConversationRequest
    {
        public Guid RecipientId { get; set; }
    }

    public class AddReactionRequest
    {
        public string Emoji { get; set; } = string.Empty;
    }

    public class SearchUsersRequest
    {
        public string Query { get; set; } = string.Empty;
    }

    public class SearchUserDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
