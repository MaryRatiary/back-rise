using Microsoft.AspNetCore.SignalR;
using Rise.API.Services;

namespace Rise.API.Hubs
{
    public class PostHub : Hub
    {
        private readonly IPostService _postService;

        public PostHub(IPostService postService)
        {
            _postService = postService;
        }

        public override async Task OnConnectedAsync()
        {
            // Joindre un groupe "posts" pour les mises à jour en temps réel
            await Groups.AddToGroupAsync(Context.ConnectionId, "posts");
            await base.OnConnectedAsync();
            Console.WriteLine($"Client connecté: {Context.ConnectionId}");
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "posts");
            await base.OnDisconnectedAsync(exception);
            Console.WriteLine($"Client déconnecté: {Context.ConnectionId}");
        }

        // Notifier tous les clients qu'un post a été aimé/désaimé
        public async Task NotifyPostLikeUpdated(Guid postId)
        {
            var post = await _postService.GetPostByIdAsync(postId);
            if (post != null)
            {
                await Clients.Group("posts").SendAsync("PostLikeUpdated", new
                {
                    postId = post.Id,
                    reactionCount = post.ReactionCount,
                    post = post
                });
            }
        }

        // Notifier tous les clients qu'un commentaire a été ajouté
        public async Task NotifyCommentAdded(Guid postId, Guid commentId)
        {
            var post = await _postService.GetPostByIdAsync(postId);
            if (post != null)
            {
                // Envoyer le commentaire complet (pas juste le postId)
                var comments = await _postService.GetPostCommentsAsync(postId);
                var newComment = comments.FirstOrDefault(c => c.Id == commentId);
                
                if (newComment != null)
                {
                    await Clients.Group("posts").SendAsync("CommentAdded", new
                    {
                        postId = postId,
                        comment = newComment,
                        commentCount = post.CommentCount
                    });
                }
            }
        }

        // Notifier quand un commentaire est supprimé
        public async Task NotifyCommentDeleted(Guid postId, Guid commentId)
        {
            var post = await _postService.GetPostByIdAsync(postId);
            if (post != null)
            {
                await Clients.Group("posts").SendAsync("CommentDeleted", new
                {
                    postId = postId,
                    commentId = commentId,
                    commentCount = post.CommentCount
                });
            }
        }

        // Notifier tous les clients qu'un commentaire a été réagi
        public async Task NotifyCommentReactionUpdated(Guid postId, Guid commentId)
        {
            var post = await _postService.GetPostByIdAsync(postId);
            if (post != null)
            {
                await Clients.Group("posts").SendAsync("CommentReactionUpdated", new
                {
                    postId = postId,
                    commentId = commentId,
                    post = post
                });
            }
        }

        // Méthode pour la synchronisation en temps réel des likes
        public async Task PostLikeToggled(Guid postId, Guid userId, bool isLiked)
        {
            var post = await _postService.GetPostByIdAsync(postId);
            if (post != null)
            {
                await Clients.Group("posts").SendAsync("PostLikeToggled", new
                {
                    postId = postId,
                    userId = userId,
                    isLiked = isLiked,
                    reactionCount = post.ReactionCount,
                    post = post
                });
            }
        }
    }
}
