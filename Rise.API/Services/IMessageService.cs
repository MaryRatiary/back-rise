using Rise.API.DTOs;

namespace Rise.API.Services
{
    public interface IMessageService
    {
        Task<IEnumerable<ConversationDto>> GetConversationsAsync(Guid userId);
        Task<IEnumerable<MessageDto>> GetMessagesAsync(Guid conversationId, Guid userId);
        Task<MessageDto> SendMessageAsync(Guid userId, CreateMessageRequest request);
        Task<ConversationDto> StartConversationAsync(Guid userId, Guid recipientId);
        Task AddReactionAsync(Guid messageId, Guid userId, string emoji);
        Task DeleteMessageAsync(Guid messageId, Guid userId);
        Task MarkConversationAsReadAsync(Guid conversationId, Guid userId);
        Task<string> GenerateCallTokenAsync(Guid conversationId, Guid userId);
        Task<IEnumerable<SearchUserDto>> SearchUsersAsync(string query, Guid excludeUserId);
    }
}
