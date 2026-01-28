using Rise.API.DTOs;
using Rise.API.Models;

namespace Rise.API.Services
{
    public interface IUserService
    {
        Task<User?> GetUserByIdAsync(Guid userId);
        Task<User> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, IFormFile? profileImage, IFormFile? coverImage);
    }
}
