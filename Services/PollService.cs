using Microsoft.EntityFrameworkCore;
using Rise.API.DTOs;
using Rise.API.Models;
using Rise.API.Data;

namespace Rise.API.Services
{
    public interface IPollService
    {
        Task<List<PollDTO>> GetAllPollsAsync(Guid? userId = null);
        Task<PollDTO?> GetPollByIdAsync(Guid pollId, Guid? userId = null);
        Task<PollDTO?> CreatePollAsync(CreatePollRequest request, Guid adminId);
        Task<bool> DeletePollAsync(Guid pollId);
        Task<bool> SubmitPollResponseAsync(Guid pollId, Guid userId, List<PollResponseRequest> responses);
    }

    public class PollService : IPollService
    {
        private readonly RiseDbContext _context;

        public PollService(RiseDbContext context)
        {
            _context = context;
        }

        public async Task<List<PollDTO>> GetAllPollsAsync(Guid? userId = null)
        {
            var polls = await _context.Polls
                .Include(p => p.Questions)
                .ThenInclude(q => q.Options)
                .ThenInclude(o => o.Responses)
                .Where(p => p.IsActive)
                .ToListAsync();

            return polls.Select(p => MapToPollDTO(p, userId)).ToList();
        }

        public async Task<PollDTO?> GetPollByIdAsync(Guid pollId, Guid? userId = null)
        {
            var poll = await _context.Polls
                .Include(p => p.Questions)
                .ThenInclude(q => q.Options)
                .ThenInclude(o => o.Responses)
                .FirstOrDefaultAsync(p => p.Id == pollId);

            return poll != null ? MapToPollDTO(poll, userId) : null;
        }

        public async Task<PollDTO?> CreatePollAsync(CreatePollRequest request, Guid adminId)
        {
            var poll = new Poll
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                TargetAudience = request.TargetAudience,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = adminId,
                IsActive = true
            };

            foreach (var questionRequest in request.Questions)
            {
                var question = new PollQuestion
                {
                    Id = Guid.NewGuid(),
                    PollId = poll.Id,
                    QuestionText = questionRequest.QuestionText,
                    AllowMultipleChoice = questionRequest.AllowMultipleChoice,
                    Order = request.Questions.IndexOf(questionRequest)
                };

                foreach (var optionText in questionRequest.Options)
                {
                    var option = new PollOption
                    {
                        Id = Guid.NewGuid(),
                        QuestionId = question.Id,
                        OptionText = optionText,
                        Order = questionRequest.Options.IndexOf(optionText)
                    };
                    question.Options.Add(option);
                }

                poll.Questions.Add(question);
            }

            _context.Polls.Add(poll);
            await _context.SaveChangesAsync();

            return await GetPollByIdAsync(poll.Id);
        }

        public async Task<bool> DeletePollAsync(Guid pollId)
        {
            var poll = await _context.Polls.FindAsync(pollId);
            if (poll == null) return false;

            _context.Polls.Remove(poll);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SubmitPollResponseAsync(Guid pollId, Guid userId, List<PollResponseRequest> responses)
        {
            foreach (var response in responses)
            {
                foreach (var optionId in response.SelectedOptionIds)
                {
                    var pollResponse = new PollResponse
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        OptionId = optionId,
                        RespondedAt = DateTime.UtcNow
                    };
                    _context.PollResponses.Add(pollResponse);
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        private PollDTO MapToPollDTO(Poll poll, Guid? userId)
        {
            var userResponses = userId.HasValue
                ? _context.PollResponses.Where(pr => pr.UserId == userId).Select(pr => pr.OptionId).ToList()
                : new List<Guid>();

            var totalResponses = poll.Questions
                .SelectMany(q => q.Options)
                .SelectMany(o => o.Responses)
                .Count();

            return new PollDTO
            {
                Id = poll.Id,
                Title = poll.Title,
                Description = poll.Description,
                StartDate = poll.StartDate,
                EndDate = poll.EndDate,
                TargetAudience = poll.TargetAudience,
                CreatedAt = poll.CreatedAt,
                IsActive = poll.IsActive,
                HasUserResponded = userResponses.Any(),
                Questions = poll.Questions.Select(q => new PollQuestionDTO
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    AllowMultipleChoice = q.AllowMultipleChoice,
                    Options = q.Options.Select(o => new PollOptionDTO
                    {
                        Id = o.Id,
                        OptionText = o.OptionText,
                        ResponseCount = o.Responses.Count,
                        Percentage = totalResponses > 0 ? (o.Responses.Count * 100.0) / totalResponses : 0
                    }).ToList()
                }).ToList()
            };
        }
    }
}
