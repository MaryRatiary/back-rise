namespace Rise.API.DTOs
{
    // Registration DTO
    public class RegisterRequest
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string ConfirmPassword { get; set; }
        public required string MatriculeNumber { get; set; } // Obligatoire et unique
        public string? Filiere { get; set; } // Optionnelle pour L1
        public required string Classe { get; set; } // L1, L2, L3, M1, M2
        public required string Role { get; set; } // Admin, Professor, Student
    }

    // Login DTO
    public class LoginRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

    // Auth Response DTO
    public class AuthResponse
    {
        public Guid UserId { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string Role { get; set; }
        public string? Filiere { get; set; }
        public required string Classe { get; set; }
        public required string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string? ProfileImageUrl { get; set; }
    }

    // User DTO
    public class UserDTO
    {
        public Guid Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string MatriculeNumber { get; set; }
        public string? Filiere { get; set; }
        public required string Classe { get; set; }
        public required string Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ProfileImageUrl { get; set; }
        public bool IsActive { get; set; }
    }

    // Update User DTO
    public class UpdateUserRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Filiere { get; set; }
        public string? Classe { get; set; }
        public string? ProfileImageUrl { get; set; }
    }

    // Update Profile Request DTO
    public class UpdateProfileRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Bio { get; set; }
        
        // Académique
        public string? Specialization { get; set; }
        
        // Professionnel
        public string? JobTitle { get; set; }
        public string? Company { get; set; }
        public string? Location { get; set; }
        
        // Listes JSON
        public string? InterestCategories { get; set; }
        public string? Associations { get; set; }
        public string? SharedExpertise { get; set; }
        public string? Languages { get; set; }
        
        // Réseaux sociaux
        public string? LinkedinUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? TwitterUrl { get; set; }
        public string? GithubUrl { get; set; }
        
        // Préférences
        public string? NotificationPreferences { get; set; }
        public string? ProfileVisibility { get; set; }
    }
}
