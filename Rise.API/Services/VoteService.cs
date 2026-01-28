using Microsoft.EntityFrameworkCore;
using Rise.API.DTOs;
using Rise.API.Models;
using Rise.API.Data;

namespace Rise.API.Services
{
    public interface IVoteService
    {
        Task<List<VoteDTO>> GetAllVotesAsync();
        Task<VoteDTO?> GetVoteByIdAsync(Guid voteId);
        Task<VoteDTO?> CreateVoteAsync(CreateVoteRequest request, Guid adminId);
        Task<bool> DeleteVoteAsync(Guid voteId);
        Task<bool> StartVoteAsync(Guid voteId);
        Task<bool> EndVoteAsync(Guid voteId);
        Task<bool> PublishResultsAsync(Guid voteId);
        Task<bool> SubmitCandidacyAsync(Guid positionId, Guid userId, string? description);
        Task<bool> ApproveCandidacyAsync(Guid optionId);
        Task<bool> CastVoteAsync(CastVoteRequest request, Guid userId);
        Task<bool> HasUserVotedAsync(Guid voteId, Guid positionId, Guid userId);
    }

    public class VoteService : IVoteService
    {
        private readonly RiseDbContext _context;

        public VoteService(RiseDbContext context)
        {
            _context = context;
        }

        public async Task<List<VoteDTO>> GetAllVotesAsync()
        {
            var votes = await _context.Votes
                .Include(v => v.Positions)
                .ThenInclude(vp => vp.Options)
                .ThenInclude(vo => vo.VotesCast)
                .ToListAsync();

            return votes.Select(MapToVoteDTO).ToList();
        }

        public async Task<VoteDTO?> GetVoteByIdAsync(Guid voteId)
        {
            var vote = await _context.Votes
                .Include(v => v.Positions)
                .ThenInclude(vp => vp.Options)
                .ThenInclude(vo => vo.VotesCast)
                .FirstOrDefaultAsync(v => v.Id == voteId);

            return vote != null ? MapToVoteDTO(vote) : null;
        }

        public async Task<VoteDTO?> CreateVoteAsync(CreateVoteRequest request, Guid adminId)
        {
            var vote = new Vote
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = adminId,
                IsActive = false,
                ResultsPublished = false
            };

            foreach (var positionRequest in request.Positions)
            {
                var position = new VotePosition
                {
                    Id = Guid.NewGuid(),
                    VoteId = vote.Id,
                    Title = positionRequest.Title,
                    Description = positionRequest.Description
                };
                vote.Positions.Add(position);
            }

            _context.Votes.Add(vote);
            await _context.SaveChangesAsync();

            return await GetVoteByIdAsync(vote.Id);
        }

        public async Task<bool> DeleteVoteAsync(Guid voteId)
        {
            var vote = await _context.Votes.FindAsync(voteId);
            if (vote == null) return false;

            _context.Votes.Remove(vote);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> StartVoteAsync(Guid voteId)
        {
            var vote = await _context.Votes.FindAsync(voteId);
            if (vote == null) return false;

            vote.IsActive = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EndVoteAsync(Guid voteId)
        {
            var vote = await _context.Votes.FindAsync(voteId);
            if (vote == null) return false;

            vote.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PublishResultsAsync(Guid voteId)
        {
            var vote = await _context.Votes.FindAsync(voteId);
            if (vote == null) return false;

            vote.ResultsPublished = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SubmitCandidacyAsync(Guid positionId, Guid userId, string? description)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            var option = new VoteOption
            {
                Id = Guid.NewGuid(),
                PositionId = positionId,
                CandidateId = userId,
                CandidateName = $"{user.FirstName} {user.LastName}",
                CandidateDescription = description,
                CandidateProfileUrl = user.ProfileImageUrl,
                SubmittedAt = DateTime.UtcNow,
                IsApproved = false
            };

            _context.VoteOptions.Add(option);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ApproveCandidacyAsync(Guid optionId)
        {
            var option = await _context.VoteOptions.FindAsync(optionId);
            if (option == null) return false;

            option.IsApproved = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CastVoteAsync(CastVoteRequest request, Guid userId)
        {
            var voteCast = new VoteCast
            {
                Id = Guid.NewGuid(),
                VoteId = request.VoteId,
                OptionId = request.OptionId,
                UserId = userId,
                CastAt = DateTime.UtcNow
            };

            _context.VotesCast.Add(voteCast);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HasUserVotedAsync(Guid voteId, Guid positionId, Guid userId)
        {
            return await _context.VotesCast
                .AnyAsync(vc => vc.VoteId == voteId && vc.UserId == userId);
        }

        private VoteDTO MapToVoteDTO(Vote vote)
        {
            return new VoteDTO
            {
                Id = vote.Id,
                Title = vote.Title,
                Description = vote.Description,
                StartDate = vote.StartDate,
                EndDate = vote.EndDate,
                CreatedAt = vote.CreatedAt,
                IsActive = vote.IsActive,
                ResultsPublished = vote.ResultsPublished,
                Positions = vote.Positions.Select(p => new VotePositionDTO
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    Options = p.Options.Select(o => new VoteOptionDTO
                    {
                        Id = o.Id,
                        CandidateName = o.CandidateName,
                        CandidateDescription = o.CandidateDescription,
                        CandidateProfileUrl = o.CandidateProfileUrl,
                        IsApproved = o.IsApproved,
                        VoteCount = o.VotesCast.Count,
                        Percentage = vote.VotesCast.Count > 0
                            ? (o.VotesCast.Count * 100.0) / vote.VotesCast.Count
                            : 0
                    }).ToList()
                }).ToList()
            };
        }
    }
}
