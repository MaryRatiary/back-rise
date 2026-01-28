using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Rise.API.DTOs;
using Rise.API.Services;
using Rise.API.Hubs;
using System.Security.Claims;

namespace Rise.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly IHubContext<PostHub> _hubContext;

        public PostsController(IPostService postService, IHubContext<PostHub> hubContext)
        {
            _postService = postService;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<ActionResult<List<PostDTO>>> GetAllPublicPosts()
        {
            var posts = await _postService.GetAllPublicPostsAsync();
            return Ok(posts);
        }

        [HttpGet("event/{eventId}")]
        public async Task<ActionResult<List<PostDTO>>> GetEventPosts(Guid eventId)
        {
            var posts = await _postService.GetEventPostsAsync(eventId);
            return Ok(posts);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public async Task<ActionResult<List<PostDTO>>> GetAllPostsForAdmin()
        {
            var posts = await _postService.GetAllPostsForAdminAsync();
            return Ok(posts);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PostDTO>> GetPostById(Guid id)
        {
            var post = await _postService.GetPostByIdAsync(id);
            if (post == null)
                return NotFound();

            return Ok(post);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<PostDTO>> CreatePost([FromBody] CreatePostRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Unauthorized();

            var post = await _postService.CreatePostAsync(request, userIdGuid);
            if (post == null)
                return BadRequest(new { message = "Failed to create post" });

            return CreatedAtAction(nameof(GetPostById), new { id = post.Id }, post);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<PostDTO>> UpdatePost(Guid id, [FromBody] CreatePostRequest request)
        {
            var post = await _postService.UpdatePostAsync(id, request);
            if (post == null)
                return NotFound();

            return Ok(post);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(Guid id)
        {
            var success = await _postService.DeletePostAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }

        [Authorize]
        [HttpPost("comment")]
        public async Task<ActionResult<CommentDTO>> AddComment([FromBody] CreateCommentRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Unauthorized();

            var comment = await _postService.AddCommentAsync(request, userIdGuid);
            if (comment == null)
                return BadRequest(new { message = "Failed to add comment" });

            // ✅ Notifier via WebSocket (pas de rechargement complet)
            // Envoyer seulement le nouveau commentaire aux clients
            await _hubContext.Clients.Group("posts").SendAsync("CommentAdded", new
            {
                postId = request.PostId,
                comment = comment,
                commentCount = (await _postService.GetPostByIdAsync(request.PostId))?.CommentCount ?? 0
            });

            return CreatedAtAction(nameof(GetPostComments), new { postId = request.PostId }, comment);
        }

        [Authorize]
        [HttpPost("reaction")]
        public async Task<IActionResult> AddReaction([FromBody] CreateReactionRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Unauthorized();

            var success = await _postService.AddReactionAsync(request, userIdGuid);
            if (!success)
                return BadRequest(new { message = "Failed to add reaction" });

            // Notifier tous les clients en temps réel via WebSocket
            if (request.PostId.HasValue)
            {
                var post = await _postService.GetPostByIdAsync(request.PostId.Value);
                if (post != null)
                {
                    // Envoyer les données complètes du post mis à jour
                    await _hubContext.Clients.Group("posts").SendAsync("PostLikeUpdated", new
                    {
                        postId = post.Id,
                        reactionCount = post.ReactionCount,
                        post = post
                    });
                }
            }
            else if (request.CommentId.HasValue)
            {
                await _hubContext.Clients.Group("posts").SendAsync("CommentReactionUpdated", new
                {
                    commentId = request.CommentId
                });
            }

            return Ok(new { message = "Reaction added successfully" });
        }

        [Authorize]
        [HttpDelete("reaction/{reactionId}")]
        public async Task<IActionResult> DeleteReaction(Guid reactionId)
        {
            var success = await _postService.DeleteReactionAsync(reactionId);
            if (!success)
                return NotFound(new { message = "Reaction not found" });

            return NoContent();
        }

        [HttpGet("{postId}/comments")]
        public async Task<ActionResult<List<CommentDTO>>> GetPostComments(Guid postId)
        {
            var comments = await _postService.GetPostCommentsAsync(postId);
            return Ok(comments);
        }

        [HttpGet("{postId}/reactions")]
        public async Task<ActionResult<List<ReactionDTO>>> GetPostReactions(Guid postId)
        {
            var reactions = await _postService.GetPostReactionsAsync(postId);
            return Ok(reactions);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("notifications")]
        public async Task<ActionResult<List<NotificationDTO>>> GetAdminNotifications()
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(adminId, out var adminIdGuid))
                return Unauthorized();

            var notifications = await _postService.GetAdminNotificationsAsync(adminIdGuid);
            return Ok(notifications);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("notifications/{notificationId}/read")]
        public async Task<IActionResult> MarkNotificationAsRead(Guid notificationId)
        {
            var success = await _postService.MarkNotificationAsReadAsync(notificationId);
            if (!success)
                return NotFound(new { message = "Notification not found" });

            return NoContent();
        }
    }
}
