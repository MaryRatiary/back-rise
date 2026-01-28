using Microsoft.EntityFrameworkCore;
using Rise.API.DTOs;
using Rise.API.Models;
using Rise.API.Data;

namespace Rise.API.Services
{
    public class UserService : IUserService
    {
        private readonly RiseDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public UserService(RiseDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<User> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, IFormFile? profileImage, IFormFile? coverImage)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new InvalidOperationException("Utilisateur non trouvé");

            // Mise à jour des informations de base
            user.FirstName = request.FirstName ?? user.FirstName;
            user.LastName = request.LastName ?? user.LastName;
            user.Email = request.Email ?? user.Email;
            user.Phone = request.Phone ?? user.Phone;
            user.Bio = request.Bio ?? user.Bio;
            
            // Informations académiques
            user.Specialization = request.Specialization ?? user.Specialization;
            
            // Informations professionnelles
            user.JobTitle = request.JobTitle ?? user.JobTitle;
            user.Company = request.Company ?? user.Company;
            user.Location = request.Location ?? user.Location;
            
            // Listes JSON
            if (!string.IsNullOrEmpty(request.InterestCategories))
                user.InterestCategories = request.InterestCategories;
            
            if (!string.IsNullOrEmpty(request.Associations))
                user.Associations = request.Associations;
            
            if (!string.IsNullOrEmpty(request.SharedExpertise))
                user.SharedExpertise = request.SharedExpertise;
            
            if (!string.IsNullOrEmpty(request.Languages))
                user.Languages = request.Languages;
            
            // Réseaux sociaux
            user.LinkedinUrl = request.LinkedinUrl ?? user.LinkedinUrl;
            user.InstagramUrl = request.InstagramUrl ?? user.InstagramUrl;
            user.TwitterUrl = request.TwitterUrl ?? user.TwitterUrl;
            user.GithubUrl = request.GithubUrl ?? user.GithubUrl;
            
            // Préférences
            if (!string.IsNullOrEmpty(request.NotificationPreferences))
                user.NotificationPreferences = request.NotificationPreferences;
            
            if (!string.IsNullOrEmpty(request.ProfileVisibility))
                user.ProfileVisibility = request.ProfileVisibility;

            // Traiter les images
            if (profileImage != null)
            {
                user.ProfileImageUrl = await SaveImageAsync(profileImage, "profile");
            }

            if (coverImage != null)
            {
                user.CoverImageUrl = await SaveImageAsync(coverImage, "cover");
            }

            user.UpdatedAt = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return user;
        }

        private async Task<string> SaveImageAsync(IFormFile file, string type)
        {
            try
            {
                // Utiliser le WebRootPath s'il existe, sinon utiliser le ContentRootPath
                var basePath = !string.IsNullOrEmpty(_environment.WebRootPath) 
                    ? _environment.WebRootPath 
                    : Path.Combine(_environment.ContentRootPath, "wwwroot");

                var uploadsDir = Path.Combine(basePath, "uploads", "profiles");
                
                // Créer le dossier s'il n'existe pas
                if (!Directory.Exists(uploadsDir))
                    Directory.CreateDirectory(uploadsDir);

                // Générer un nom de fichier unique avec l'extension d'origine
                var fileExtension = Path.GetExtension(file.FileName);
                var fileName = $"{Guid.NewGuid()}_{type}{fileExtension}";
                var filePath = Path.Combine(uploadsDir, fileName);

                // Sauvegarder le fichier
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Retourner l'URL relative accessible publiquement
                var imageUrl = $"/uploads/profiles/{fileName}";
                System.Diagnostics.Debug.WriteLine($"✅ Image sauvegardée: {imageUrl}");
                return imageUrl;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur lors de la sauvegarde de l'image: {ex.Message}");
                throw new InvalidOperationException($"Erreur lors de la sauvegarde de l'image: {ex.Message}", ex);
            }
        }
    }
}
