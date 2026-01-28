using Microsoft.EntityFrameworkCore;
using Rise.API.DTOs;
using Rise.API.Models;
using Rise.API.Data;
using Rise.API.Enums;

namespace Rise.API.Services
{
    public interface IEventService
    {
        Task<List<EventDTO>> GetAllEventsAsync(Guid? userId = null);
        Task<List<EventDTO>> GetAllEventsForAdminAsync();
        Task<EventDTO?> GetEventByIdAsync(Guid eventId, Guid? userId = null);
        Task<EventDTO?> CreateEventAsync(CreateEventRequest request, Guid adminId);
        Task<EventDTO?> UpdateEventAsync(Guid eventId, UpdateEventRequest request, Guid adminId);
        Task<bool> DeleteEventAsync(Guid eventId);
        Task<bool> PublishEventAsync(Guid eventId);
        Task<bool> RegisterToEventAsync(Guid eventId, Guid userId, Guid? formSubmissionId = null);
        Task<bool> UnregisterFromEventAsync(Guid eventId, Guid userId);
        Task<List<EventRegistrationDTO>> GetUserRegistrationsAsync(Guid userId);
        Task<List<FormSubmissionDTO>> GetEventRegistrationsWithFormAsync(Guid eventId);
    }

    public class EventService : IEventService
    {
        private readonly RiseDbContext _context;
        private readonly IPostService _postService;

        public EventService(RiseDbContext context, IPostService postService)
        {
            _context = context;
            _postService = postService;
        }

        public async Task<List<EventDTO>> GetAllEventsAsync(Guid? userId = null)
        {
            try
            {
                var events = await _context.Events
                    .Include(e => e.Registrations)
                    .Include(e => e.Form)
                    .ThenInclude(f => f!.Questions)
                    .ThenInclude(q => q.Options)
                    .Include(e => e.Form)
                    .ThenInclude(f => f!.Submissions)
                    .Include(e => e.Posts)
                    .ThenInclude(p => p.Images)
                    .Where(e => e.IsPublished)
                    .ToListAsync();

                return events.Select(e => MapToEventDTO(e, userId)).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllEventsAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<List<EventDTO>> GetAllEventsForAdminAsync()
        {
            try
            {
                var events = await _context.Events
                    .Include(e => e.Registrations)
                    .Include(e => e.Form)
                    .ThenInclude(f => f!.Questions)
                    .ThenInclude(q => q.Options)
                    .Include(e => e.Form)
                    .ThenInclude(f => f!.Submissions)
                    .Include(e => e.Posts)
                    .ThenInclude(p => p.Images)
                    .OrderByDescending(e => e.CreatedAt)
                    .ToListAsync();

                return events.Select(e => MapToEventDTO(e, null)).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllEventsForAdminAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<EventDTO?> GetEventByIdAsync(Guid eventId, Guid? userId = null)
        {
            try
            {
                var @event = await _context.Events
                    .Include(e => e.Registrations)
                    .Include(e => e.Form)
                    .ThenInclude(f => f!.Questions)
                    .ThenInclude(q => q.Options)
                    .Include(e => e.Form)
                    .ThenInclude(f => f!.Submissions)
                    .Include(e => e.Posts)
                    .ThenInclude(p => p.Images)
                    .FirstOrDefaultAsync(e => e.Id == eventId);

                return @event != null ? MapToEventDTO(@event, userId) : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetEventByIdAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<EventDTO?> CreateEventAsync(CreateEventRequest request, Guid adminId)
        {
            try
            {
                // Convertir le type d'événement depuis la chaîne envoyée
                EventType eventType = ParseEventType(request.Type);

                // Convertir les dates en UTC
                var startDate = DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc);
                var endDate = DateTime.SpecifyKind(request.EndDate, DateTimeKind.Utc);

                var @event = new Event
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Description = request.Description,
                    Type = eventType,
                    StartDate = startDate,
                    EndDate = endDate,
                    Location = request.Location,
                    Theme = request.Theme,
                    MaxParticipants = request.MaxParticipants,
                    PosterUrl = request.PosterUrl,
                    DocumentUrl = request.DocumentUrl ?? (request.DocumentUrls?.FirstOrDefault()),
                    Rules = request.Rules,
                    FormId = request.FormId,
                    RequireFormSubmission = request.RequireFormSubmission,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = adminId,
                    IsPublished = false
                };

                _context.Events.Add(@event);
                await _context.SaveChangesAsync();

                // Si des images ont été fournies, créer un post avec ces images
                if (request.ImageUrls != null && request.ImageUrls.Count > 0)
                {
                    var postRequest = new CreatePostRequest
                    {
                        EventId = @event.Id,
                        Content = $"Publication pour l'événement: {@event.Name}",
                        ImageUrls = request.ImageUrls,
                        VideoUrl = null,
                        ExternalLink = null
                    };

                    await _postService.CreatePostAsync(postRequest, adminId);
                }

                return MapToEventDTO(@event, null);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Erreur lors de la création de l'événement: {ex.Message}", ex);
            }
        }

        private EventType ParseEventType(string typeString)
        {
            // Mapper les chaînes du frontend vers les valeurs de l'enum
            return typeString.ToLower() switch
            {
                var s when s.Contains("hackathon") => EventType.Hackathon,
                var s when s.Contains("excursion") || s.Contains("sortie") => EventType.Excursion,
                var s when s.Contains("intégration") || s.Contains("integration") => EventType.Integration,
                var s when s.Contains("étudiant") || s.Contains("student") || s.Contains("échange") => EventType.StudentExchange,
                var s when s.Contains("vote") || s.Contains("président") || s.Contains("president") || s.Contains("election") => EventType.Election,
                var s when s.Contains("sondage") || s.Contains("survey") => EventType.Survey,
                _ => EventType.Other
            };
        }

        public async Task<EventDTO?> UpdateEventAsync(Guid eventId, UpdateEventRequest request, Guid adminId)
        {
            var @event = await _context.Events.FindAsync(eventId);
            if (@event == null) return null;

            if (!string.IsNullOrEmpty(request.Name)) @event.Name = request.Name;
            if (!string.IsNullOrEmpty(request.Description)) @event.Description = request.Description;
            if (request.StartDate.HasValue) @event.StartDate = request.StartDate.Value;
            if (request.EndDate.HasValue) @event.EndDate = request.EndDate.Value;
            if (!string.IsNullOrEmpty(request.Location)) @event.Location = request.Location;
            if (!string.IsNullOrEmpty(request.Theme)) @event.Theme = request.Theme;
            if (request.MaxParticipants.HasValue) @event.MaxParticipants = request.MaxParticipants;
            if (!string.IsNullOrEmpty(request.PosterUrl)) @event.PosterUrl = request.PosterUrl;
            if (!string.IsNullOrEmpty(request.DocumentUrl)) @event.DocumentUrl = request.DocumentUrl;
            if (!string.IsNullOrEmpty(request.Rules)) @event.Rules = request.Rules;
            if (request.FormId.HasValue) @event.FormId = request.FormId;
            if (request.RequireFormSubmission.HasValue) @event.RequireFormSubmission = request.RequireFormSubmission.Value;

            @event.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Ajouter les images au post existant de l'événement
            if (request.ImageUrls != null && request.ImageUrls.Count > 0)
            {
                // Chercher le post existant pour cet événement
                var existingPost = await _context.Posts
                    .Include(p => p.Images)
                    .FirstOrDefaultAsync(p => p.EventId == eventId);

                if (existingPost != null)
                {
                    // Ajouter les nouvelles images au post existant
                    int maxOrder = existingPost.Images.Count > 0 ? existingPost.Images.Max(i => i.DisplayOrder) + 1 : 0;
                    
                    for (int i = 0; i < request.ImageUrls.Count; i++)
                    {
                        var newImage = new PostImage
                        {
                            Id = Guid.NewGuid(),
                            PostId = existingPost.Id,
                            ImageUrl = request.ImageUrls[i],
                            DisplayOrder = maxOrder + i,
                            CreatedAt = DateTime.UtcNow
                        };
                        existingPost.Images.Add(newImage);
                        _context.PostImages.Add(newImage);
                    }

                    existingPost.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // Si aucun post n'existe, en créer un nouveau
                    var postRequest = new CreatePostRequest
                    {
                        EventId = eventId,
                        Content = $"Événement: {@event.Name}",
                        ImageUrls = request.ImageUrls,
                        VideoUrl = null,
                        ExternalLink = null
                    };

                    await _postService.CreatePostAsync(postRequest, adminId);
                }
            }

            @event = await _context.Events
                .Include(e => e.Registrations)
                .FirstAsync(e => e.Id == eventId);

            return MapToEventDTO(@event, null);
        }

        public async Task<bool> DeleteEventAsync(Guid eventId)
        {
            var @event = await _context.Events.FindAsync(eventId);
            if (@event == null) return false;

            _context.Events.Remove(@event);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PublishEventAsync(Guid eventId)
        {
            var @event = await _context.Events.FindAsync(eventId);
            if (@event == null) return false;

            @event.IsPublished = true;
            @event.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RegisterToEventAsync(Guid eventId, Guid userId, Guid? formSubmissionId = null)
        {
            // Vérifier si l'événement existe et s'il demande une soumission de formulaire
            var @event = await _context.Events.FindAsync(eventId);
            if (@event == null) return false;

            // Si le formulaire est requis et aucune soumission n'est fournie, retourner false
            if (@event.RequireFormSubmission && !formSubmissionId.HasValue)
                return false;

            // Vérifier si l'utilisateur est déjà inscrit
            var existingRegistration = await _context.EventRegistrations
                .FirstOrDefaultAsync(er => er.EventId == eventId && er.UserId == userId);
            
            if (existingRegistration != null)
                return false; // L'utilisateur est déjà inscrit

            var registration = new EventRegistration
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                UserId = userId,
                RegisteredAt = DateTime.UtcNow,
                IsAttended = false,
                FormSubmissionId = formSubmissionId
            };

            _context.EventRegistrations.Add(registration);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnregisterFromEventAsync(Guid eventId, Guid userId)
        {
            var registration = await _context.EventRegistrations
                .FirstOrDefaultAsync(er => er.EventId == eventId && er.UserId == userId);

            if (registration == null) return false;

            _context.EventRegistrations.Remove(registration);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<EventRegistrationDTO>> GetUserRegistrationsAsync(Guid userId)
        {
            var registrations = await _context.EventRegistrations
                .Include(er => er.Event)
                .Where(er => er.UserId == userId)
                .ToListAsync();

            return registrations.Select(r => new EventRegistrationDTO
            {
                Id = r.Id,
                EventId = r.EventId,
                EventName = r.Event.Name,
                RegisteredAt = r.RegisteredAt,
                IsAttended = r.IsAttended,
                FormSubmissionId = r.FormSubmissionId
            }).ToList();
        }

        // Nouvelle méthode : Récupérer les inscriptions avec formulaire
        public async Task<List<FormSubmissionDTO>> GetEventRegistrationsWithFormAsync(Guid eventId)
        {
            var submissions = await _context.FormSubmissions
                .Include(fs => fs.User)
                .Include(fs => fs.Answers)
                .ThenInclude(a => a.Question)
                .Include(fs => fs.Answers)
                .ThenInclude(a => a.Option)
                .Include(fs => fs.EventRegistration)
                .Where(fs => fs.EventRegistration != null && fs.EventRegistration.EventId == eventId)
                .ToListAsync();

            return submissions.Select(fs => MapToFormSubmissionDTO(fs)).ToList();
        }

        private FormSubmissionDTO MapToFormSubmissionDTO(FormSubmission submission)
        {
            return new FormSubmissionDTO
            {
                Id = submission.Id,
                UserId = submission.UserId,
                UserName = $"{submission.User?.FirstName ?? "N/A"} {submission.User?.LastName ?? "N/A"}",
                UserEmail = submission.User?.Email ?? "N/A",
                SubmittedAt = submission.SubmittedAt,
                Answers = submission.Answers.Select(a => new AnswerDTO
                {
                    QuestionId = a.QuestionId,
                    QuestionTitle = a.Question?.Title ?? "N/A",
                    QuestionType = a.Question?.Type ?? QuestionType.ShortText,
                    OptionText = a.Option?.OptionText,
                    ResponseValue = a.ResponseValue
                }).ToList()
            };
        }

        private EventDTO MapToEventDTO(Event @event, Guid? userId)
        {
            // Récupérer les images depuis les posts liés à l'événement
            var imageUrls = new List<string>();
            
            // Si l'événement a des posts associés, récupérer les images du premier post
            if (@event.Posts != null && @event.Posts.Count > 0)
            {
                var firstPost = @event.Posts.FirstOrDefault();
                if (firstPost?.Images != null && firstPost.Images.Count > 0)
                {
                    imageUrls = firstPost.Images
                        .OrderBy(i => i.DisplayOrder)
                        .Select(i => i.ImageUrl)
                        .ToList();
                }
            }

            return new EventDTO
            {
                Id = @event.Id,
                Name = @event.Name,
                Description = @event.Description,
                Type = @event.Type.ToString(),
                StartDate = @event.StartDate,
                EndDate = @event.EndDate,
                Location = @event.Location,
                Theme = @event.Theme,
                MaxParticipants = @event.MaxParticipants,
                RegisteredCount = @event.Registrations.Count,
                PosterUrl = @event.PosterUrl,
                DocumentUrl = @event.DocumentUrl,
                Rules = @event.Rules,
                CreatedAt = @event.CreatedAt,
                IsPublished = @event.IsPublished,
                IsUserRegistered = userId.HasValue && @event.Registrations.Any(r => r.UserId == userId),
                ImageUrls = imageUrls,
                FormId = @event.FormId,
                RequireFormSubmission = @event.RequireFormSubmission,
                Form = @event.Form != null ? MapToFormDTO(@event.Form) : null
            };
        }

        private FormDTO MapToFormDTO(Form form)
        {
            return new FormDTO
            {
                Id = form.Id,
                Title = form.Title,
                Description = form.Description,
                CreatedAt = form.CreatedAt,
                UpdatedAt = form.UpdatedAt,
                StartDate = form.StartDate,
                EndDate = form.EndDate,
                IsPublished = form.IsPublished,
                IsActive = form.IsActive,
                TargetAudience = form.TargetAudience,
                AllowMultipleResponses = form.AllowMultipleResponses,
                TotalResponses = form.Submissions.Count,
                Questions = form.Questions.OrderBy(q => q.Order).Select(q => new FormQuestionDTO
                {
                    Id = q.Id,
                    Title = q.Title,
                    Description = q.Description,
                    Type = q.Type,
                    IsRequired = q.IsRequired,
                    Order = q.Order,
                    Options = q.Options.OrderBy(o => o.Order).Select(o => new QuestionOptionDTO
                    {
                        Id = o.Id,
                        OptionText = o.OptionText,
                        Order = o.Order,
                        ResponseCount = 0
                    }).ToList()
                }).ToList()
            };
        }
    }
}
