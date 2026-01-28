using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rise.API.DTOs;
using Rise.API.Services;

namespace Rise.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessagesController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        [HttpGet("conversations")]
        public async Task<ActionResult<IEnumerable<ConversationDto>>> GetConversations()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var conversations = await _messageService.GetConversationsAsync(Guid.Parse(userId));
            return Ok(conversations);
        }

        [HttpGet("conversation/{conversationId}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessages(Guid conversationId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var messages = await _messageService.GetMessagesAsync(conversationId, Guid.Parse(userId));
            return Ok(messages);
        }

        [HttpPost("send")]
        public async Task<ActionResult<MessageDto>> SendMessage([FromBody] CreateMessageRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var message = await _messageService.SendMessageAsync(Guid.Parse(userId), request);
            return Ok(message);
        }

        [HttpPost("conversations")]
        public async Task<ActionResult<ConversationDto>> StartConversation([FromBody] StartConversationRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var conversation = await _messageService.StartConversationAsync(Guid.Parse(userId), request.RecipientId);
            return Ok(conversation);
        }

        [HttpPost("{messageId}/reaction")]
        public async Task<ActionResult> AddReaction(Guid messageId, [FromBody] AddReactionRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            await _messageService.AddReactionAsync(messageId, Guid.Parse(userId), request.Emoji);
            return Ok();
        }

        [HttpDelete("{messageId}")]
        public async Task<ActionResult> DeleteMessage(Guid messageId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            await _messageService.DeleteMessageAsync(messageId, Guid.Parse(userId));
            return Ok();
        }

        [HttpPut("conversation/{conversationId}/read")]
        public async Task<ActionResult> MarkAsRead(Guid conversationId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            await _messageService.MarkConversationAsReadAsync(conversationId, Guid.Parse(userId));
            return Ok();
        }

        [HttpPost("conversation/{conversationId}/call-token")]
        public async Task<ActionResult> GetCallToken(Guid conversationId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var token = await _messageService.GenerateCallTokenAsync(conversationId, Guid.Parse(userId));
            return Ok(new { token });
        }
    }
}
