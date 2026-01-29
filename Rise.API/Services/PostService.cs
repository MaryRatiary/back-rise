using Microsoft.EntityFrameworkCore;
using Rise.API.DTOs;
using Rise.API.Models;
using Rise.API.Data;
using Rise.API.Enums;

namespace Rise.API.Services
{
    public interface IPostService
    {
        Task<List<PostDTO>> GetAllPublicPostsAsync();
        Task<List<PostDTO>> GetEventPostsAsync(Guid eventId);
        Task<List<PostDTO>> GetAllPostsForAdminAsync();
        Task<PostDTO?> GetPostByIdAsync(Guid postId);
        Task<PostDTO?> CreatePostAsync(CreatePostRequest request, Guid userId);
        Task<PostDTO?> UpdatePostAsync(Guid postId, CreatePostRequest request);
        Task<bool> DeletePostAsync(Guid postId);
        Task<CommentDTO?> AddCommentAsync(CreateCommentRequest request, Guid userId);
        Task<bool> AddReactionAsync(CreateReactionRequest request, Guid userId);
        Task<bool> DeleteReactionAsync(Guid reactionId);
        Task<List<CommentDTO>> GetPostCommentsAsync(Guid postId);
        Task<List<ReactionDTO>> GetPostReactionsAsync(Guid postId);
        Task<List<NotificationDTO>> GetAdminNotificationsAsync(Guid adminId);
        Task<bool> MarkNotificationAsReadAsync(Guid notificationId);
    }

    public class PostService : IPostService
    {
        private readonly RiseDbContext _context;

        public PostService(RiseDbContext context)
        {
            _context = context;
        }

        public async Task<List<PostDTO>> GetAllPublicPostsAsync()
        {
            var posts = await _context.Posts
                .Include(p => p.Images)
                .Include(p => p.Reactions)
                .Where(p => p.IsPublic)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            // Charger les commentaires et réactions séparément
            var postIds = posts.Select(p => p.Id).ToList();
            var comments = await _context.Comments
                .Where(c => postIds.Contains(c.PostId))
                .Include(c => c.User)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.User)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.Reactions)
                .Include(c => c.TaggedUsers)
                .Include(c => c.Reactions)
                    .ThenInclude(r => r.User)
                .ToListAsync();

            // ✅ KEY FIX: Charger SEULEMENT les users utilisés, pas TOUS les users
            var userIds = posts.Select(p => p.CreatedBy)
                .Concat(comments.Select(c => c.UserId))
                .Concat(comments.SelectMany(c => c.Replies.Select(r => r.UserId)))
                .Concat(comments.SelectMany(c => c.TaggedUsers.Select(t => t.TaggedUserId)))
                .Concat(comments.SelectMany(c => c.Reactions.Select(r => r.UserId)))
                .Concat(comments.SelectMany(c => c.Replies.SelectMany(r => r.Reactions.Select(re => re.UserId))))
                .Distinct()
                .ToList();
            
            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id)) // ← FILTRE!
                .ToDictionaryAsync(u => u.Id);

            // Associer les commentaires aux posts
            foreach (var post in posts)
            {
                post.Comments = comments.Where(c => c.PostId == post.Id).ToList();
            }

            return posts.Select(p => MapToPostDTO(p, users)).ToList();
        }

        public async Task<List<PostDTO>> GetEventPostsAsync(Guid eventId)
        {
            var posts = await _context.Posts
                .Include(p => p.Images)
                .Include(p => p.Reactions)
                .Where(p => p.EventId == eventId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var postIds = posts.Select(p => p.Id).ToList();
            var comments = await _context.Comments
                .Where(c => postIds.Contains(c.PostId))
                .Include(c => c.User)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.User)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.Reactions)
                .Include(c => c.TaggedUsers)
                .Include(c => c.Reactions)
                    .ThenInclude(r => r.User)
                .ToListAsync();

            // ✅ KEY FIX: Charger SEULEMENT les users utilisés
            var userIds = posts.Select(p => p.CreatedBy)
                .Concat(comments.Select(c => c.UserId))
                .Concat(comments.SelectMany(c => c.Replies.Select(r => r.UserId)))
                .Concat(comments.SelectMany(c => c.TaggedUsers.Select(t => t.TaggedUserId)))
                .Concat(comments.SelectMany(c => c.Reactions.Select(r => r.UserId)))
                .Concat(comments.SelectMany(c => c.Replies.SelectMany(r => r.Reactions.Select(re => re.UserId))))
                .Distinct()
                .ToList();

            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id);

            foreach (var post in posts)
            {
                post.Comments = comments.Where(c => c.PostId == post.Id).ToList();
            }

            return posts.Select(p => MapToPostDTO(p, users)).ToList();
        }

        public async Task<List<PostDTO>> GetAllPostsForAdminAsync()
        {
            var posts = await _context.Posts
                .Include(p => p.Images)
                .Include(p => p.Reactions)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var postIds = posts.Select(p => p.Id).ToList();
            var comments = await _context.Comments
                .Where(c => postIds.Contains(c.PostId))
                .Include(c => c.User)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.User)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.Reactions)
                .Include(c => c.TaggedUsers)
                .Include(c => c.Reactions)
                    .ThenInclude(r => r.User)
                .ToListAsync();

            // ✅ KEY FIX: Charger SEULEMENT les users utilisés
            var userIds = posts.Select(p => p.CreatedBy)
                .Concat(comments.Select(c => c.UserId))
                .Concat(comments.SelectMany(c => c.Replies.Select(r => r.UserId)))
                .Concat(comments.SelectMany(c => c.TaggedUsers.Select(t => t.TaggedUserId)))
                .Concat(comments.SelectMany(c => c.Reactions.Select(r => r.UserId)))
                .Concat(comments.SelectMany(c => c.Replies.SelectMany(r => r.Reactions.Select(re => re.UserId))))
                .Distinct()
                .ToList();

            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id);

            foreach (var post in posts)
            {
                post.Comments = comments.Where(c => c.PostId == post.Id).ToList();
            }

            return posts.Select(p => MapToPostDTO(p, users)).ToList();
        }

        public async Task<PostDTO?> GetPostByIdAsync(Guid postId)
        {
            var post = await _context.Posts
                .Include(p => p.Images)
                .Include(p => p.Reactions)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null) return null;

            // Charger les commentaires séparément
            post.Comments = await _context.Comments
                .Where(c => c.PostId == postId)
                .Include(c => c.User)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.User)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.Reactions)
                .Include(c => c.TaggedUsers)
                .Include(c => c.Reactions)
                    .ThenInclude(r => r.User)
                .ToListAsync();

            // ✅ KEY FIX: Charger SEULEMENT les users utilisés
            var userIds = new List<Guid> { post.CreatedBy }
                .Concat(post.Comments.Select(c => c.UserId))
                .Concat(post.Comments.SelectMany(c => c.Replies.Select(r => r.UserId)))
                .Concat(post.Comments.SelectMany(c => c.TaggedUsers.Select(t => t.TaggedUserId)))
                .Concat(post.Comments.SelectMany(c => c.Reactions.Select(r => r.UserId)))
                .Concat(post.Comments.SelectMany(c => c.Replies.SelectMany(r => r.Reactions.Select(re => re.UserId))))
                .Distinct()
                .ToList();

            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id);

            return MapToPostDTO(post, users);
        }

        public async Task<PostDTO?> CreatePostAsync(CreatePostRequest request, Guid userId)
        {
            // Validate that EventId exists if provided
            Guid? validEventId = null;
            if (request.EventId.HasValue && request.EventId != Guid.Empty)
            {
                var eventExists = await _context.Events.AnyAsync(e => e.Id == request.EventId);
                if (!eventExists)
                {
                    // If event doesn't exist, set to null instead of causing FK violation
                    validEventId = null;
                }
                else
                {
                    validEventId = request.EventId;
                }
            }

            var post = new Post
            {
                Id = Guid.NewGuid(),
                EventId = validEventId,
                CreatedBy = userId,
                Content = request.Content,
                VideoUrl = request.VideoUrl,
                ExternalLink = request.ExternalLink,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ReactionCount = 0,
                CommentCount = 0,
                IsPublic = true
            };

            if (request.ImageUrls != null && request.ImageUrls.Count > 0)
            {
                for (int i = 0; i < request.ImageUrls.Count; i++)
                {
                    post.Images.Add(new PostImage
                    {
                        Id = Guid.NewGuid(),
                        PostId = post.Id,
                        ImageUrl = request.ImageUrls[i],
                        DisplayOrder = i,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return await GetPostByIdAsync(post.Id);
        }

        public async Task<PostDTO?> UpdatePostAsync(Guid postId, CreatePostRequest request)
        {
            var post = await _context.Posts
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null) return null;

            post.Content = request.Content;
            post.VideoUrl = request.VideoUrl;
            post.ExternalLink = request.ExternalLink;
            post.UpdatedAt = DateTime.UtcNow;

            if (request.ImageUrls != null && request.ImageUrls.Count > 0)
            {
                int maxOrder = post.Images.Count > 0 ? post.Images.Max(i => i.DisplayOrder) + 1 : 0;
                
                for (int i = 0; i < request.ImageUrls.Count; i++)
                {
                    var newImage = new PostImage
                    {
                        Id = Guid.NewGuid(),
                        PostId = postId,
                        ImageUrl = request.ImageUrls[i],
                        DisplayOrder = maxOrder + i,
                        CreatedAt = DateTime.UtcNow
                    };
                    post.Images.Add(newImage);
                    _context.PostImages.Add(newImage);
                }
            }

            _context.Posts.Update(post);
            await _context.SaveChangesAsync();

            return await GetPostByIdAsync(postId);
        }

        public async Task<bool> DeletePostAsync(Guid postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null) return false;

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CommentDTO?> AddCommentAsync(CreateCommentRequest request, Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            var post = await _context.Posts.FindAsync(request.PostId);
            if (post == null) return null;

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                PostId = request.PostId,
                UserId = userId,
                Content = request.Content,
                ParentCommentId = request.ParentCommentId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);

            // Ajouter les tags
            if (request.TaggedUsernames != null && request.TaggedUsernames.Count > 0)
            {
                var taggedUsers = await _context.Users
                    .Where(u => request.TaggedUsernames.Contains($"{u.FirstName} {u.LastName}"))
                    .ToListAsync();

                foreach (var taggedUser in taggedUsers)
                {
                    comment.TaggedUsers.Add(new TaggedUser
                    {
                        Id = Guid.NewGuid(),
                        CommentId = comment.Id,
                        TaggedUserId = taggedUser.Id,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            post.CommentCount++;

            // Créer les notifications
            var postCreator = await _context.Users.FindAsync(post.CreatedBy);
            if (postCreator != null)
            {
                // Notification pour le créateur du post
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    RecipientId = post.CreatedBy,
                    Type = "comment",
                    TriggeredByUserId = userId,
                    Message = $"{user.FirstName} {user.LastName} a commenté votre post",
                    PostId = request.PostId,
                    CommentId = comment.Id,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Notifications.Add(notification);
            }

            // Notifications pour les utilisateurs tagués
            if (request.TaggedUsernames != null && request.TaggedUsernames.Count > 0)
            {
                var taggedUsers = await _context.Users
                    .Where(u => request.TaggedUsernames.Contains($"{u.FirstName} {u.LastName}"))
                    .ToListAsync();

                foreach (var taggedUser in taggedUsers)
                {
                    if (taggedUser.Id != userId)
                    {
                        var notification = new Notification
                        {
                            Id = Guid.NewGuid(),
                            RecipientId = taggedUser.Id,
                            Type = "mention",
                            TriggeredByUserId = userId,
                            Message = $"{user.FirstName} {user.LastName} vous a mentionné dans un commentaire",
                            PostId = request.PostId,
                            CommentId = comment.Id,
                            IsRead = false,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Notifications.Add(notification);
                    }
                }
            }

            // Notification pour les admins
            var admins = await _context.Users
                .Where(u => u.Role == UserRole.Admin)
                .ToListAsync();

            foreach (var admin in admins)
            {
                if (admin.Id != userId)
                {
                    var notification = new Notification
                    {
                        Id = Guid.NewGuid(),
                        RecipientId = admin.Id,
                        Type = "comment",
                        TriggeredByUserId = userId,
                        Message = $"{user.FirstName} {user.LastName} a commenté un post",
                        PostId = request.PostId,
                        CommentId = comment.Id,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Notifications.Add(notification);
                }
            }

            await _context.SaveChangesAsync();

            return MapToCommentDTO(comment, user, new List<User>());
        }

        public async Task<bool> AddReactionAsync(CreateReactionRequest request, Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            // Gestion des likes sur les posts
            if (request.PostId.HasValue)
            {
                var post = await _context.Posts.FindAsync(request.PostId);
                if (post == null) return false;

                // Initialiser la liste si elle est null
                if (post.LikedByUserIds == null)
                    post.LikedByUserIds = new List<Guid>();

                bool userAlreadyLiked = post.LikedByUserIds.Contains(userId);

                if (userAlreadyLiked)
                {
                    // Retirer le like (unlike)
                    post.LikedByUserIds.Remove(userId);
                    if (post.ReactionCount > 0)
                        post.ReactionCount--;
                }
                else
                {
                    // Ajouter le like
                    post.LikedByUserIds.Add(userId);
                    post.ReactionCount++;

                    // Notification pour le créateur du post
                    if (post.CreatedBy != userId)
                    {
                        var notification = new Notification
                        {
                            Id = Guid.NewGuid(),
                            RecipientId = post.CreatedBy,
                            Type = "reaction",
                            TriggeredByUserId = userId,
                            Message = $"{user.FirstName} {user.LastName} a aimé votre post",
                            PostId = post.Id,
                            IsRead = false,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Notifications.Add(notification);
                    }

                    // Notifications pour les admins
                    var admins = await _context.Users
                        .Where(u => u.Role == UserRole.Admin)
                        .ToListAsync();

                    foreach (var admin in admins)
                    {
                        if (admin.Id != userId)
                        {
                            var notification = new Notification
                            {
                                Id = Guid.NewGuid(),
                                RecipientId = admin.Id,
                                Type = "reaction",
                                TriggeredByUserId = userId,
                                Message = $"{user.FirstName} {user.LastName} a aimé un contenu",
                                PostId = request.PostId,
                                IsRead = false,
                                CreatedAt = DateTime.UtcNow
                            };
                            _context.Notifications.Add(notification);
                        }
                    }
                }

                _context.Posts.Update(post);
                await _context.SaveChangesAsync();
            }
            // Gestion des likes sur les commentaires
            else if (request.CommentId.HasValue)
            {
                var comment = await _context.Comments.FindAsync(request.CommentId);
                if (comment == null) return false;

                var existingReaction = await _context.Reactions
                    .FirstOrDefaultAsync(r => r.CommentId == request.CommentId && r.UserId == userId && r.PostId == null);

                if (existingReaction != null)
                {
                    // Supprimer la réaction (unlike)
                    _context.Reactions.Remove(existingReaction);
                    if (comment.ReactionCount > 0)
                        comment.ReactionCount--;
                }
                else
                {
                    // Créer une nouvelle réaction (like)
                    comment.ReactionCount++;

                    var reaction = new Reaction
                    {
                        Id = Guid.NewGuid(),
                        PostId = null,
                        CommentId = request.CommentId,
                        UserId = userId,
                        EmojiType = request.EmojiType,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Reactions.Add(reaction);

                    // Notification pour le créateur du commentaire
                    if (comment.UserId != userId)
                    {
                        var notification = new Notification
                        {
                            Id = Guid.NewGuid(),
                            RecipientId = comment.UserId,
                            Type = "reaction",
                            TriggeredByUserId = userId,
                            Message = $"{user.FirstName} {user.LastName} a aimé votre commentaire",
                            PostId = comment.PostId,
                            CommentId = comment.Id,
                            IsRead = false,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Notifications.Add(notification);
                    }

                    // Notifications pour les admins
                    var admins = await _context.Users
                        .Where(u => u.Role == UserRole.Admin)
                        .ToListAsync();

                    foreach (var admin in admins)
                    {
                        if (admin.Id != userId)
                        {
                            var notification = new Notification
                            {
                                Id = Guid.NewGuid(),
                                RecipientId = admin.Id,
                                Type = "reaction",
                                TriggeredByUserId = userId,
                                Message = $"{user.FirstName} {user.LastName} a aimé un contenu",
                                PostId = null,
                                CommentId = request.CommentId,
                                IsRead = false,
                                CreatedAt = DateTime.UtcNow
                            };
                            _context.Notifications.Add(notification);
                        }
                    }
                }

                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> DeleteReactionAsync(Guid reactionId)
        {
            var reaction = await _context.Reactions.FindAsync(reactionId);
            if (reaction == null) return false;

            if (reaction.PostId.HasValue)
            {
                var post = await _context.Posts.FindAsync(reaction.PostId);
                if (post != null) post.ReactionCount--;
            }
            else if (reaction.CommentId.HasValue)
            {
                var comment = await _context.Comments.FindAsync(reaction.CommentId);
                if (comment != null) comment.ReactionCount--;
            }

            _context.Reactions.Remove(reaction);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<CommentDTO>> GetPostCommentsAsync(Guid postId)
        {
            var comments = await _context.Comments
                .Include(c => c.User)
                .Include(c => c.TaggedUsers)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.User)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.TaggedUsers)
                .Include(c => c.Reactions)
                    .ThenInclude(r => r.User)
                .Where(c => c.PostId == postId && c.ParentCommentId == null)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var users = await _context.Users.ToDictionaryAsync(u => u.Id);

            return comments.Select(c => MapToCommentDTO(c, c.User, users.Values.ToList())).ToList();
        }

        public async Task<List<ReactionDTO>> GetPostReactionsAsync(Guid postId)
        {
            var reactions = await _context.Reactions
                .Include(r => r.User)
                .Where(r => r.PostId == postId && r.CommentId == null)
                .ToListAsync();

            return reactions.Select(r => new ReactionDTO
            {
                Id = r.Id,
                UserId = r.UserId,
                UserName = $"{r.User.FirstName} {r.User.LastName}",
                EmojiType = r.EmojiType,
                CreatedAt = r.CreatedAt
            }).ToList();
        }

        public async Task<List<NotificationDTO>> GetAdminNotificationsAsync(Guid adminId)
        {
            var notifications = await _context.Notifications
                .Include(n => n.TriggeredByUser)
                .Where(n => n.RecipientId == adminId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return notifications.Select(n => new NotificationDTO
            {
                Id = n.Id,
                Type = n.Type,
                TriggeredByUserId = n.TriggeredByUserId,
                TriggeredByUserName = $"{n.TriggeredByUser.FirstName} {n.TriggeredByUser.LastName}",
                Message = n.Message,
                PostId = n.PostId,
                CommentId = n.CommentId,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            }).ToList();
        }

        public async Task<bool> MarkNotificationAsReadAsync(Guid notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification == null) return false;

            notification.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }

        private PostDTO MapToPostDTO(Post post, Dictionary<Guid, User> users)
        {
            var createdByName = users.TryGetValue(post.CreatedBy, out var user)
                ? $"{user.FirstName} {user.LastName}"
                : "Unknown";
            
            var createdByProfileImage = users.TryGetValue(post.CreatedBy, out var userForImage)
                ? userForImage.ProfileImageUrl
                : null;

            var commentDtos = post.Comments
                .Where(c => c.ParentCommentId == null)
                .Select(c => MapToCommentDTO(c, c.User, users.Values.ToList()))
                .ToList();

            return new PostDTO
            {
                Id = post.Id,
                EventId = post.EventId,
                CreatedBy = post.CreatedBy,
                CreatedByName = createdByName,
                CreatedByProfileImage = createdByProfileImage,
                Content = post.Content,
                Images = post.Images
                    .OrderBy(i => i.DisplayOrder)
                    .Select(i => new PostImageDTO
                    {
                        Id = i.Id,
                        ImageUrl = i.ImageUrl,
                        DisplayOrder = i.DisplayOrder
                    }).ToList(),
                VideoUrl = post.VideoUrl,
                ExternalLink = post.ExternalLink,
                CreatedAt = post.CreatedAt,
                CommentCount = post.CommentCount,
                ReactionCount = post.ReactionCount,
                LikedByUserIds = post.LikedByUserIds ?? new List<Guid>(),
                Comments = commentDtos,
                Reactions = post.Reactions
                    .Where(r => r.CommentId == null)
                    .Select(r => new ReactionDTO
                    {
                        Id = r.Id,
                        UserId = r.UserId,
                        UserName = $"{r.User.FirstName} {r.User.LastName}",
                        EmojiType = r.EmojiType,
                        CreatedAt = r.CreatedAt
                    }).ToList()
            };
        }

        private CommentDTO MapToCommentDTO(Comment comment, User user, List<User> allUsers)
        {
            var replies = comment.Replies
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new CommentDTO
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    UserName = $"{r.User.FirstName} {r.User.LastName}",
                    UserProfileImageUrl = r.User.ProfileImageUrl,
                    Content = r.Content,
                    CreatedAt = r.CreatedAt,
                    ReactionCount = r.ReactionCount,
                    ParentCommentId = r.ParentCommentId,
                    TaggedUsers = r.TaggedUsers.Select(t => new TaggedUserDTO
                    {
                        Id = t.Id,
                        TaggedUserId = t.TaggedUserId,
                        Username = $"{allUsers.FirstOrDefault(u => u.Id == t.TaggedUserId)?.FirstName} {allUsers.FirstOrDefault(u => u.Id == t.TaggedUserId)?.LastName}" ?? "Unknown"
                    }).ToList(),
                    Reactions = r.Reactions.Select(re => new ReactionDTO
                    {
                        Id = re.Id,
                        UserId = re.UserId,
                        UserName = $"{re.User.FirstName} {re.User.LastName}",
                        EmojiType = re.EmojiType,
                        CreatedAt = re.CreatedAt
                    }).ToList()
                }).ToList();

            return new CommentDTO
            {
                Id = comment.Id,
                UserId = comment.UserId,
                UserName = $"{user.FirstName} {user.LastName}",
                UserProfileImageUrl = user.ProfileImageUrl,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                ReactionCount = comment.ReactionCount,
                ParentCommentId = comment.ParentCommentId,
                Replies = replies,
                TaggedUsers = comment.TaggedUsers.Select(t => new TaggedUserDTO
                {
                    Id = t.Id,
                    TaggedUserId = t.TaggedUserId,
                    Username = $"{allUsers.FirstOrDefault(u => u.Id == t.TaggedUserId)?.FirstName} {allUsers.FirstOrDefault(u => u.Id == t.TaggedUserId)?.LastName}" ?? "Unknown"
                }).ToList(),
                Reactions = comment.Reactions.Select(r => new ReactionDTO
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    UserName = $"{r.User.FirstName} {r.User.LastName}",
                    EmojiType = r.EmojiType,
                    CreatedAt = r.CreatedAt
                }).ToList()
            };
        }
    }
}
