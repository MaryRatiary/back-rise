namespace Rise.API.DTOs
{
    public class CreatePostRequest
    {
        public Guid? EventId { get; set; }
        public required string Content { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>();
        public string? VideoUrl { get; set; }
        public string? ExternalLink { get; set; }
    }

    public class PostImageDTO
    {
        public Guid Id { get; set; }
        public required string ImageUrl { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class PostDTO
    {
        public Guid Id { get; set; }
        public Guid? EventId { get; set; }
        public Guid CreatedBy { get; set; }
        public required string CreatedByName { get; set; }
        public string? CreatedByProfileImage { get; set; }
        public required string Content { get; set; }
        public List<PostImageDTO> Images { get; set; } = new List<PostImageDTO>();
        public string? VideoUrl { get; set; }
        public string? ExternalLink { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CommentCount { get; set; }
        public int ReactionCount { get; set; }
        public List<Guid>? LikedByUserIds { get; set; } = new List<Guid>();
        public List<CommentDTO> Comments { get; set; } = new List<CommentDTO>();
        public List<ReactionDTO> Reactions { get; set; } = new List<ReactionDTO>();
    }

    public class CreateCommentRequest
    {
        public Guid PostId { get; set; }
        public required string Content { get; set; }
        public Guid? ParentCommentId { get; set; } // Pour les réponses aux commentaires
        public List<string> TaggedUsernames { get; set; } = new List<string>(); // @mentions
    }

    public class CommentDTO
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public required string UserName { get; set; }
        public string? UserProfileImageUrl { get; set; }
        public required string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ReactionCount { get; set; }
        public Guid? ParentCommentId { get; set; }
        public List<CommentDTO> Replies { get; set; } = new List<CommentDTO>();
        public List<TaggedUserDTO> TaggedUsers { get; set; } = new List<TaggedUserDTO>();
        public List<ReactionDTO> Reactions { get; set; } = new List<ReactionDTO>();
    }

    public class TaggedUserDTO
    {
        public Guid Id { get; set; }
        public Guid TaggedUserId { get; set; }
        public required string Username { get; set; }
    }

    public class CreateReactionRequest
    {
        public Guid? PostId { get; set; }
        public Guid? CommentId { get; set; }
        public required string EmojiType { get; set; } // like, love, haha, wow, sad, angry
    }

    public class ReactionDTO
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public required string UserName { get; set; }
        public required string EmojiType { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class NotificationDTO
    {
        public Guid Id { get; set; }
        public required string Type { get; set; } // "comment", "reaction", "mention", "reply"
        public Guid TriggeredByUserId { get; set; }
        public required string TriggeredByUserName { get; set; }
        public required string Message { get; set; }
        public Guid? PostId { get; set; }
        public Guid? CommentId { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
