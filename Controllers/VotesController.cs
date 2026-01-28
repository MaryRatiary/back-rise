using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rise.API.DTOs;
using Rise.API.Services;
using System.Security.Claims;

namespace Rise.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VotesController : ControllerBase
    {
        private readonly IVoteService _voteService;

        public VotesController(IVoteService voteService)
        {
            _voteService = voteService;
        }

        [HttpGet]
        public async Task<ActionResult<List<VoteDTO>>> GetAllVotes()
        {
            var votes = await _voteService.GetAllVotesAsync();
            return Ok(votes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VoteDTO>> GetVoteById(Guid id)
        {
            var vote = await _voteService.GetVoteByIdAsync(id);
            if (vote == null)
                return NotFound();

            return Ok(vote);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<VoteDTO>> CreateVote([FromBody] CreateVoteRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Unauthorized();

            var vote = await _voteService.CreateVoteAsync(request, userIdGuid);
            if (vote == null)
                return BadRequest(new { message = "Failed to create vote" });

            return CreatedAtAction(nameof(GetVoteById), new { id = vote.Id }, vote);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVote(Guid id)
        {
            var success = await _voteService.DeleteVoteAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/start")]
        public async Task<IActionResult> StartVote(Guid id)
        {
            var success = await _voteService.StartVoteAsync(id);
            if (!success)
                return NotFound();

            return Ok(new { message = "Vote started successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/end")]
        public async Task<IActionResult> EndVote(Guid id)
        {
            var success = await _voteService.EndVoteAsync(id);
            if (!success)
                return NotFound();

            return Ok(new { message = "Vote ended successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/publish-results")]
        public async Task<IActionResult> PublishResults(Guid id)
        {
            var success = await _voteService.PublishResultsAsync(id);
            if (!success)
                return NotFound();

            return Ok(new { message = "Results published successfully" });
        }

        [Authorize]
        [HttpPost("submit-candidacy")]
        public async Task<IActionResult> SubmitCandidacy([FromBody] SubmitCandidacyRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Unauthorized();

            var success = await _voteService.SubmitCandidacyAsync(request.PositionId, userIdGuid, request.CandidateDescription);
            if (!success)
                return BadRequest(new { message = "Failed to submit candidacy" });

            return Ok(new { message = "Candidacy submitted successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("approve-candidacy/{optionId}")]
        public async Task<IActionResult> ApproveCandidacy(Guid optionId)
        {
            var success = await _voteService.ApproveCandidacyAsync(optionId);
            if (!success)
                return NotFound();

            return Ok(new { message = "Candidacy approved successfully" });
        }

        [Authorize]
        [HttpPost("cast-vote")]
        public async Task<IActionResult> CastVote([FromBody] CastVoteRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Unauthorized();

            var success = await _voteService.CastVoteAsync(request, userIdGuid);
            if (!success)
                return BadRequest(new { message = "Failed to cast vote" });

            return Ok(new { message = "Vote cast successfully" });
        }
    }
}
