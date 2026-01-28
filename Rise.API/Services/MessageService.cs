using Microsoft.EntityFrameworkCore;
using Rise.API.Data;
using Rise.API.DTOs;
using Rise.API.Models;
using System.Text;

namespace Rise.API.Services
{
    public class MessageService : IMessageService
    {
        private readonly RiseDbContext _context;
        private readonly ILogger<MessageService> _logger;

        public MessageService(RiseDbContext context, ILogger<MessageService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<ConversationDto>> GetConversationsAsync(Guid userId)
        {
            try
            {
                _logger.LogInformation($"Getting conversations for user {userId}");
                
                var conversations = await _context.Conversations
                    .Where(c => c.UserId1 == userId || c.UserId2 == userId)
                    .Include(c => c.User1)
                    .Include(c => c.User2)
                    .OrderByDescending(c => c.LastMessageAt)
                    .ToListAsync();

                _logger.LogInformation($"Found {conversations.Count} conversations");

                var result = new List<ConversationDto>();

                foreach (var conv in conversations)
                {
                    try
                    {
                        var otherUser = conv.UserId1 == userId ? conv.User2 : conv.User1;
                        
                        if (otherUser == null)
                        {
                            _logger.LogWarning($"Conversation {conv.Id} has null user");
                            continue;
                        }

                        // Get last message for this conversation
                        var lastMessage = await _context.Messages
                            .Where(m => m.ConversationId == conv.Id)
                            .OrderByDescending(m => m.SentAt)
                            .FirstOrDefaultAsync();

                        var unreadCount = await _context.Messages
                            .Where(m => m.ConversationId == conv.Id && !m.IsRead && m.SenderId != userId)
                            .CountAsync();

                        result.Add(new ConversationDto
                        {
                            Id = conv.Id,
                            Sender = $"{otherUser.FirstName} {otherUser.LastName}",
                            SenderProfileImageUrl = otherUser.ProfileImageUrl ?? string.Empty,
                            LastMessage = lastMessage?.Content ?? "Pas de message",
                            Time = lastMessage?.SentAt ?? conv.CreatedAt,
                            Unread = unreadCount,
                            Messages = new List<MessageDto>() // Don't load all messages here
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing conversation {conv.Id}: {ex.Message}");
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetConversationsAsync: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        public async Task<IEnumerable<MessageDto>> GetMessagesAsync(Guid conversationId, Guid userId)
        {
            var messages = await _context.Messages
                .Where(m => m.ConversationId == conversationId)
                .Include(m => m.Sender)
                .Include(m => m.ReplyTo)
                .ThenInclude(m => m!.Sender)
                .Include(m => m.Reactions)
                .ThenInclude(r => r.User)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            return messages.Select(m => MapMessageToDto(m, userId)).ToList();
        }

        public async Task<MessageDto> SendMessageAsync(Guid userId, CreateMessageRequest request)
        {
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.Id == request.ConversationId &&
                    (c.UserId1 == userId || c.UserId2 == userId));

            if (conversation == null)
                throw new InvalidOperationException("Conversation not found or unauthorized");

            var message = new Message
            {
                Id = Guid.NewGuid(),
                ConversationId = request.ConversationId,
                SenderId = userId,
                Content = request.Content,
                SentAt = DateTime.UtcNow,
                ReplyToId = request.ReplyToId,
                IsRead = false
            };

            _context.Messages.Add(message);
            conversation.LastMessageAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Load the message with relationships for DTO mapping
            await _context.Messages
                .Where(m => m.Id == message.Id)
                .Include(m => m.Sender)
                .Include(m => m.ReplyTo)
                .ThenInclude(m => m!.Sender)
                .Include(m => m.Reactions)
                .ThenInclude(r => r.User)
                .LoadAsync();

            return MapMessageToDto(message, userId);
        }

        public async Task<ConversationDto> StartConversationAsync(Guid userId, Guid recipientId)
        {
            // Check if conversation already exists
            var existingConversation = await _context.Conversations
                .FirstOrDefaultAsync(c =>
                    (c.UserId1 == userId && c.UserId2 == recipientId) ||
                    (c.UserId1 == recipientId && c.UserId2 == userId));

            if (existingConversation != null)
            {
                return await GetConversationDetailsAsync(existingConversation.Id, userId);
            }

            // Create new conversation
            var conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                UserId1 = userId,
                UserId2 = recipientId,
                CreatedAt = DateTime.UtcNow,
                LastMessageAt = DateTime.UtcNow
            };

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            return await GetConversationDetailsAsync(conversation.Id, userId);
        }

        public async Task AddReactionAsync(Guid messageId, Guid userId, string emoji)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message == null)
                throw new InvalidOperationException("Message not found");

            // Check if user already reacted with this emoji
            var existingReaction = await _context.MessageReactions
                .FirstOrDefaultAsync(r => r.MessageId == messageId &&
                    r.UserId == userId &&
                    r.Emoji == emoji);

            if (existingReaction != null)
            {
                // Remove if exists (toggle)
                _context.MessageReactions.Remove(existingReaction);
            }
            else
            {
                // Add new reaction
                var reaction = new MessageReaction
                {
                    Id = Guid.NewGuid(),
                    MessageId = messageId,
                    UserId = userId,
                    Emoji = emoji,
                    CreatedAt = DateTime.UtcNow
                };
                _context.MessageReactions.Add(reaction);
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteMessageAsync(Guid messageId, Guid userId)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message == null || message.SenderId != userId)
                throw new InvalidOperationException("Message not found or unauthorized");

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
        }

        public async Task MarkConversationAsReadAsync(Guid conversationId, Guid userId)
        {
            var messages = await _context.Messages
                .Where(m => m.ConversationId == conversationId && m.SenderId != userId)
                .ToListAsync();

            foreach (var message in messages)
            {
                message.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<string> GenerateCallTokenAsync(Guid conversationId, Guid userId)
        {
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.Id == conversationId &&
                    (c.UserId1 == userId || c.UserId2 == userId));

            if (conversation == null)
                throw new InvalidOperationException("Conversation not found");

            // Generate a simple token (in production, use proper JWT or Agora/Twilio tokens)
            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes(
                $"{conversationId}:{userId}:{DateTime.UtcNow.Ticks}"
            ));

            return token;
        }

        public async Task<IEnumerable<SearchUserDto>> SearchUsersAsync(string query, Guid excludeUserId)
        {
            var users = await _context.Users
                .Where(u => u.Id != excludeUserId &&
                    (u.FirstName.ToLower().Contains(query.ToLower()) ||
                     u.LastName.ToLower().Contains(query.ToLower()) ||
                     u.Email.ToLower().Contains(query.ToLower())))
                .Take(10)
                .Select(u => new SearchUserDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email
                })
                .ToListAsync();

            return users;
        }

        private MessageDto MapMessageToDto(Message message, Guid userId)
        {
            return new MessageDto
            {
                Id = message.Id,
                ConversationId = message.ConversationId,
                SenderId = message.SenderId,
                SenderName = message.Sender != null
                    ? $"{message.Sender.FirstName} {message.Sender.LastName}"
                    : "Unknown",
                SenderProfileImageUrl = message.Sender?.ProfileImageUrl ?? string.Empty,
                Content = message.Content,
                SentAt = message.SentAt,
                IsRead = message.IsRead,
                IsOwn = message.SenderId == userId,
                ReplyTo = message.ReplyTo != null ? new MessageReplyDto
                {
                    Id = message.ReplyTo.Id,
                    Sender = message.ReplyTo.Sender != null
                        ? $"{message.ReplyTo.Sender.FirstName} {message.ReplyTo.Sender.LastName}"
                        : "Unknown",
                    Content = message.ReplyTo.Content
                } : null,
                Reactions = message.Reactions != null && message.Reactions.Any()
                    ? message.Reactions
                        .GroupBy(r => r.Emoji)
                        .Select(g => new MessageReactionDto
                        {
                            Id = g.First().Id,
                            Emoji = g.Key,
                            User = string.Join(", ", g.Select(r => r.User?.FirstName ?? "Unknown")),
                            CreatedAt = g.First().CreatedAt
                        })
                        .ToList()
                    : new List<MessageReactionDto>()
            };
        }

        private async Task<ConversationDto> GetConversationDetailsAsync(Guid conversationId, Guid userId)
        {
            var conversation = await _context.Conversations
                .Include(c => c.User1)
                .Include(c => c.User2)
                .Include(c => c.Messages)
                .ThenInclude(m => m.Sender)
                .Include(c => c.Messages)
                .ThenInclude(m => m.ReplyTo)
                .ThenInclude(m => m!.Sender)
                .Include(c => c.Messages)
                .ThenInclude(m => m.Reactions)
                .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(c => c.Id == conversationId);

            if (conversation == null)
                throw new InvalidOperationException("Conversation not found");

            var otherUser = conversation.UserId1 == userId ? conversation.User2 : conversation.User1;

            return new ConversationDto
            {
                Id = conversation.Id,
                Sender = $"{otherUser.FirstName} {otherUser.LastName}",
                SenderProfileImageUrl = otherUser.ProfileImageUrl ?? string.Empty,
                LastMessage = conversation.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault()?.Content ?? "Pas de message",
                Time = conversation.LastMessageAt,
                Unread = 0,
                Messages = conversation.Messages
                    .OrderBy(m => m.SentAt)
                    .Select(m => MapMessageToDto(m, userId))
                    .ToList()
            };
        }
    }
}
