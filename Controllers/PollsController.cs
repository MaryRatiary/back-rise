using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rise.API.DTOs;
using Rise.API.Services;
using System.Security.Claims;

namespace Rise.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PollsController : ControllerBase
    {
        private readonly IPollService _pollService;

        public PollsController(IPollService pollService)
        {
            _pollService = pollService;
        }

        [HttpGet]
        public async Task<ActionResult<List<PollDTO>>> GetAllPolls()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userIdGuid = userId != null ? Guid.Parse(userId) : (Guid?)null;
            var polls = await _pollService.GetAllPollsAsync(userIdGuid);
            return Ok(polls);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PollDTO>> GetPollById(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userIdGuid = userId != null ? Guid.Parse(userId) : (Guid?)null;
            var poll = await _pollService.GetPollByIdAsync(id, userIdGuid);

            if (poll == null)
                return NotFound();

            return Ok(poll);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<PollDTO>> CreatePoll([FromBody] CreatePollRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Unauthorized();

            var poll = await _pollService.CreatePollAsync(request, userIdGuid);
            if (poll == null)
                return BadRequest(new { message = "Failed to create poll" });

            return CreatedAtAction(nameof(GetPollById), new { id = poll.Id }, poll);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePoll(Guid id)
        {
            var success = await _pollService.DeletePollAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }

        [Authorize]
        [HttpPost("{id}/respond")]
        public async Task<IActionResult> RespondToPoll(Guid id, [FromBody] List<PollResponseRequest> responses)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out var userIdGuid))
                return Unauthorized();

            var success = await _pollService.SubmitPollResponseAsync(id, userIdGuid, responses);
            if (!success)
                return BadRequest(new { message = "Failed to submit response" });

            return Ok(new { message = "Response submitted successfully" });
        }
    }
}
