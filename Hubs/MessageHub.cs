using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Rise.API.DTOs;
using Rise.API.Services;

namespace Rise.API.Hubs
{
    [Authorize]
    public class MessageHub : Hub
    {
        private readonly IMessageService _messageService;
        private readonly ILogger<MessageHub> _logger;

        // Store user connection mappings: userId -> connectionId
        private static Dictionary<string, List<string>> UserConnections = new();

        public MessageHub(IMessageService messageService, ILogger<MessageHub> logger)
        {
            _messageService = messageService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (!string.IsNullOrEmpty(userId))
            {
                if (!UserConnections.ContainsKey(userId))
                {
                    UserConnections[userId] = new List<string>();
                }
                UserConnections[userId].Add(Context.ConnectionId);

                _logger.LogInformation($"User {userId} connected with connection ID {Context.ConnectionId}");
                
                // Notify others that user is online
                await Clients.Others.SendAsync("UserOnline", userId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (!string.IsNullOrEmpty(userId) && UserConnections.ContainsKey(userId))
            {
                UserConnections[userId].Remove(Context.ConnectionId);
                
                if (UserConnections[userId].Count == 0)
                {
                    UserConnections.Remove(userId);
                    // Notify others that user is offline
                    await Clients.Others.SendAsync("UserOffline", userId);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Join a conversation room
        public async Task JoinConversation(string conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation-{conversationId}");
            _logger.LogInformation($"Connection {Context.ConnectionId} joined conversation {conversationId}");
        }

        // Leave a conversation room
        public async Task LeaveConversation(string conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation-{conversationId}");
            _logger.LogInformation($"Connection {Context.ConnectionId} left conversation {conversationId}");
        }

        // Send a message
        public async Task SendMessage(string conversationId, string content, string? replyToId = null)
        {
            try
            {
                var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    await Clients.Caller.SendAsync("Error", "Unauthorized");
                    return;
                }

                var request = new CreateMessageRequest
                {
                    ConversationId = Guid.Parse(conversationId),
                    Content = content,
                    ReplyToId = string.IsNullOrEmpty(replyToId) ? null : Guid.Parse(replyToId)
                };

                var message = await _messageService.SendMessageAsync(Guid.Parse(userId), request);

                // Send to sender with isOwn = true
                await Clients.Caller.SendAsync("MessageReceived", new
                {
                    message.Id,
                    message.ConversationId,
                    message.SenderId,
                    message.SenderName,
                    message.Content,
                    message.SentAt,
                    message.IsRead,
                    isOwn = true,
                    message.ReplyTo,
                    message.Reactions
                });

                // Send to others with isOwn = false
                await Clients.OthersInGroup($"conversation-{conversationId}")
                    .SendAsync("MessageReceived", new
                    {
                        message.Id,
                        message.ConversationId,
                        message.SenderId,
                        message.SenderName,
                        message.Content,
                        message.SentAt,
                        message.IsRead,
                        isOwn = false,
                        message.ReplyTo,
                        message.Reactions
                    });

                _logger.LogInformation($"Message sent in conversation {conversationId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending message: {ex.Message}");
                await Clients.Caller.SendAsync("Error", $"Failed to send message: {ex.Message}");
            }
        }

        // Add reaction to message
        public async Task AddReaction(string messageId, string emoji)
        {
            try
            {
                var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    await Clients.Caller.SendAsync("Error", "Unauthorized");
                    return;
                }

                await _messageService.AddReactionAsync(Guid.Parse(messageId), Guid.Parse(userId), emoji);

                // Broadcast to all connected clients
                await Clients.All.SendAsync("ReactionAdded", new { messageId, emoji, userId });

                _logger.LogInformation($"Reaction added to message {messageId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding reaction: {ex.Message}");
                await Clients.Caller.SendAsync("Error", $"Failed to add reaction: {ex.Message}");
            }
        }

        // Delete message
        public async Task DeleteMessage(string messageId)
        {
            try
            {
                var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    await Clients.Caller.SendAsync("Error", "Unauthorized");
                    return;
                }

                await _messageService.DeleteMessageAsync(Guid.Parse(messageId), Guid.Parse(userId));

                // Broadcast deletion
                await Clients.All.SendAsync("MessageDeleted", messageId);

                _logger.LogInformation($"Message {messageId} deleted");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting message: {ex.Message}");
                await Clients.Caller.SendAsync("Error", $"Failed to delete message: {ex.Message}");
            }
        }

        // Typing indicator
        public async Task UserTyping(string conversationId, string userName)
        {
            await Clients.Group($"conversation-{conversationId}")
                .SendAsync("UserTyping", new { userName });
        }

        public async Task UserStoppedTyping(string conversationId, string userName)
        {
            await Clients.Group($"conversation-{conversationId}")
                .SendAsync("UserStoppedTyping", new { userName });
        }

        // Get online users
        public async Task GetOnlineUsers()
        {
            var onlineUsers = UserConnections.Keys.ToList();
            await Clients.Caller.SendAsync("OnlineUsers", onlineUsers);
        }

        // Check if a user is online
        public async Task CheckUserOnline(string userId)
        {
            var isOnline = UserConnections.ContainsKey(userId);
            await Clients.Caller.SendAsync("UserStatus", new { userId, isOnline });
        }
    }
}
