using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Rise.API.DTOs;
using Rise.API.Models;
using Rise.API.Data;
using Rise.API.Enums;

namespace Rise.API.Services
{
    public interface IAuthService
    {
        Task<AuthResponse?> RegisterAsync(RegisterRequest request);
        Task<AuthResponse?> LoginAsync(LoginRequest request);
        string GenerateToken(User user);
    }

    public class AuthService : IAuthService
    {
        private readonly RiseDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(RiseDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
        {
            if (request.Password != request.ConfirmPassword)
                throw new InvalidOperationException("Les mots de passe ne correspondent pas");

            // Valider que l'email est unique
            var existingEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existingEmail != null)
                throw new InvalidOperationException("Cet email est déjà utilisé. Veuillez en choisir un autre.");

            // Valider que le numéro de matricule est unique
            var existingMatricule = await _context.Users.FirstOrDefaultAsync(u => u.MatriculeNumber == request.MatriculeNumber);
            if (existingMatricule != null)
                throw new InvalidOperationException("Ce numéro de matricule est déjà utilisé. Veuillez en choisir un autre.");

            // Validation : Filière est requise pour L2, L3, M1, M2 mais optionnelle pour L1
            if (request.Classe != "L1" && string.IsNullOrEmpty(request.Filiere))
                throw new InvalidOperationException("La filière est requise pour les classes L2, L3, M1 et M2");

            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                MatriculeNumber = request.MatriculeNumber,
                Filiere = request.Classe == "L1" ? null : request.Filiere,
                Classe = request.Classe,
                Role = Enum.Parse<UserRole>(request.Role),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                ProfileVisibility = "public"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role.ToString(),
                Filiere = user.Filiere,
                Classe = user.Classe,
                Token = GenerateToken(user),
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                ProfileImageUrl = user.ProfileImageUrl
            };
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return null;

            return new AuthResponse
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role.ToString(),
                Filiere = user.Filiere,
                Classe = user.Classe,
                Token = GenerateToken(user),
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                ProfileImageUrl = user.ProfileImageUrl
            };
        }

        public string GenerateToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
