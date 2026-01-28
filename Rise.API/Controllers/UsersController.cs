using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rise.API.DTOs;
using Rise.API.Services;
using System.Security.Claims;

namespace Rise.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly IUserService _userService;

        public UsersController(IMessageService messageService, IUserService userService)
        {
            _messageService = messageService;
            _userService = userService;
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<SearchUserDto>>> SearchUsers([FromQuery] string q = "")
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var results = await _messageService.SearchUsersAsync(q, Guid.Parse(userId));
            return Ok(results);
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<dynamic>> GetUserProfile(string userId)
        {
            try
            {
                if (!Guid.TryParse(userId, out var userIdGuid))
                    return BadRequest(new { message = "Invalid user ID format" });

                var user = await _userService.GetUserByIdAsync(userIdGuid);
                if (user == null)
                    return NotFound(new { message = "Utilisateur non trouvé" });

                return Ok(new
                {
                    id = user.Id,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    email = user.Email,
                    phone = user.Phone,
                    bio = user.Bio,
                    classe = user.Classe,
                    filiere = user.Filiere,
                    specialization = user.Specialization,
                    jobTitle = user.JobTitle,
                    company = user.Company,
                    location = user.Location,
                    profileImageUrl = user.ProfileImageUrl,
                    coverImageUrl = user.CoverImageUrl,
                    interestCategories = user.InterestCategories,
                    associations = user.Associations,
                    sharedExpertise = user.SharedExpertise,
                    languages = user.Languages,
                    linkedinUrl = user.LinkedinUrl,
                    instagramUrl = user.InstagramUrl,
                    twitterUrl = user.TwitterUrl,
                    githubUrl = user.GithubUrl,
                    notificationPreferences = user.NotificationPreferences,
                    profileVisibility = user.ProfileVisibility,
                    eventsJoined = user.EventsJoined,
                    eventsAttended = user.EventsAttended,
                    badges = user.Badges,
                    role = user.Role.ToString(),
                    matriculeNumber = user.MatriculeNumber,
                    createdAt = user.CreatedAt,
                    updatedAt = user.UpdatedAt,
                    isActive = user.IsActive
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("profile")]
        public async Task<ActionResult<dynamic>> UpdateProfile([FromForm] UpdateProfileRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            try
            {
                var profileImage = Request.Form.Files.FirstOrDefault(f => f.Name == "profileImage");
                var coverImage = Request.Form.Files.FirstOrDefault(f => f.Name == "coverImage");

                var updatedUser = await _userService.UpdateProfileAsync(Guid.Parse(userId), request, profileImage, coverImage);

                return Ok(new
                {
                    id = updatedUser.Id,
                    firstName = updatedUser.FirstName,
                    lastName = updatedUser.LastName,
                    email = updatedUser.Email,
                    phone = updatedUser.Phone,
                    bio = updatedUser.Bio,
                    classe = updatedUser.Classe,
                    filiere = updatedUser.Filiere,
                    specialization = updatedUser.Specialization,
                    jobTitle = updatedUser.JobTitle,
                    company = updatedUser.Company,
                    location = updatedUser.Location,
                    profileImageUrl = updatedUser.ProfileImageUrl,
                    coverImageUrl = updatedUser.CoverImageUrl,
                    interestCategories = updatedUser.InterestCategories,
                    associations = updatedUser.Associations,
                    sharedExpertise = updatedUser.SharedExpertise,
                    languages = updatedUser.Languages,
                    linkedinUrl = updatedUser.LinkedinUrl,
                    instagramUrl = updatedUser.InstagramUrl,
                    twitterUrl = updatedUser.TwitterUrl,
                    githubUrl = updatedUser.GithubUrl,
                    notificationPreferences = updatedUser.NotificationPreferences,
                    profileVisibility = updatedUser.ProfileVisibility,
                    eventsJoined = updatedUser.EventsJoined,
                    eventsAttended = updatedUser.EventsAttended,
                    badges = updatedUser.Badges,
                    role = updatedUser.Role.ToString(),
                    matriculeNumber = updatedUser.MatriculeNumber,
                    createdAt = updatedUser.CreatedAt,
                    updatedAt = updatedUser.UpdatedAt,
                    isActive = updatedUser.IsActive
                });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
