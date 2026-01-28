using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rise.API.Data;
using Rise.API.DTOs;
using Rise.API.Enums;
using Rise.API.Services;
using System.Security.Claims;

namespace Rise.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly RiseDbContext _context;
        private readonly IEventService _eventService;
        private readonly IPollService _pollService;
        private readonly IVoteService _voteService;
        private readonly IPostService _postService;

        public AdminController(
            RiseDbContext context,
            IEventService eventService,
            IPollService pollService,
            IVoteService voteService,
            IPostService postService)
        {
            _context = context;
            _eventService = eventService;
            _pollService = pollService;
            _voteService = voteService;
            _postService = postService;
        }

        // Users Management
        [HttpGet("users")]
        public async Task<ActionResult<List<UserDTO>>> GetAllUsers()
        {
            var users = await _context.Users
                .Where(u => u.IsActive)
                .Select(u => new UserDTO
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    MatriculeNumber = u.MatriculeNumber,
                    Filiere = u.Filiere,
                    Classe = u.Classe,
                    Role = u.Role.ToString(),
                    CreatedAt = u.CreatedAt,
                    ProfileImageUrl = u.ProfileImageUrl,
                    IsActive = u.IsActive
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet("users/{id}")]
        public async Task<ActionResult<UserDTO>> GetUserById(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            return Ok(new UserDTO
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                MatriculeNumber = user.MatriculeNumber,
                Filiere = user.Filiere,
                Classe = user.Classe,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt,
                ProfileImageUrl = user.ProfileImageUrl,
                IsActive = user.IsActive
            });
        }

        [HttpPut("users/{id}")]
        public async Task<ActionResult<UserDTO>> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            if (!string.IsNullOrEmpty(request.FirstName)) user.FirstName = request.FirstName;
            if (!string.IsNullOrEmpty(request.LastName)) user.LastName = request.LastName;
            if (!string.IsNullOrEmpty(request.Filiere)) user.Filiere = request.Filiere;
            if (!string.IsNullOrEmpty(request.Classe)) user.Classe = request.Classe;
            if (!string.IsNullOrEmpty(request.ProfileImageUrl)) user.ProfileImageUrl = request.ProfileImageUrl;

            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new UserDTO
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                MatriculeNumber = user.MatriculeNumber,
                Filiere = user.Filiere,
                Classe = user.Classe,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt,
                ProfileImageUrl = user.ProfileImageUrl,
                IsActive = user.IsActive
            });
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            user.IsActive = false;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Utilisateur désactivé avec succès" });
        }

        // Statistics
        [HttpGet("statistics")]
        public async Task<ActionResult> GetStatistics()
        {
            try
            {
                var totalUsers = await _context.Users.CountAsync(u => u.IsActive);
                var totalEvents = await _context.Events.CountAsync();
                var totalPolls = await _context.Polls.CountAsync();
                var totalVotes = await _context.Votes.CountAsync();
                var totalRegistrations = await _context.EventRegistrations.CountAsync();

                // Calcul sécurisé du taux de participation
                double eventParticipation = 0;
                if (totalEvents > 0 && totalUsers > 0)
                {
                    eventParticipation = Math.Round((totalRegistrations * 100.0) / (totalEvents * totalUsers), 2);
                }

                // Comptage par rôle en utilisant l'enum directement
                var studentCount = await _context.Users.CountAsync(u => u.Role == UserRole.Student && u.IsActive);
                var professorCount = await _context.Users.CountAsync(u => u.Role == UserRole.Professor && u.IsActive);
                var adminCount = await _context.Users.CountAsync(u => u.Role == UserRole.Admin && u.IsActive);

                return Ok(new
                {
                    totalUsers,
                    totalEvents,
                    totalPolls,
                    totalVotes,
                    totalRegistrations,
                    eventParticipationRate = eventParticipation,
                    studentCount,
                    professorCount,
                    adminCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors du calcul des statistiques", details = ex.Message });
            }
        }
    }
}
