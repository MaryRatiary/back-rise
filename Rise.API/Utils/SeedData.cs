using Rise.API.Data;
using Rise.API.Models;
using Rise.API.Enums;

namespace Rise.API.Utils
{
    public static class SeedData
    {
        public static void InitializeAdminUser(RiseDbContext context)
        {
            // Vérifier si un admin existe déjà par email OU par matricule
            var adminExists = context.Users.Any(u => 
                u.Email == "maryratiary12@gmail.com" || 
                u.MatriculeNumber == "ADMIN001");
            
            if (adminExists)
            {
                Console.WriteLine("✅ L'utilisateur admin existe déjà.");
            }
            else
            {
                var admin = new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Mario",
                    LastName = "Ratiaty",
                    Email = "maryratiary12@gmail.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!@"),
                    MatriculeNumber = "ADMIN001",
                    Filiere = null,
                    Classe = "L1",
                    Role = UserRole.Admin,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                context.Users.Add(admin);
                context.SaveChanges();
                
                Console.WriteLine("✅ Utilisateur admin créé avec succès!");
                Console.WriteLine($"📧 Email: maryratiary12@gmail.com");
                Console.WriteLine($"🔐 Password: Admin123!@");
                Console.WriteLine($"👤 Rôle: Admin");
            }

            // Ajouter des posts de test
            InitializeSamplePosts(context);
        }

        private static void InitializeSamplePosts(RiseDbContext context)
        {
            // Vérifier s'il y a déjà des posts
            if (context.Posts.Any())
            {
                Console.WriteLine("✅ Les publications de test existent déjà.");
                return;
            }

            var admin = context.Users.FirstOrDefault(u => u.Email == "maryratiary12@gmail.com");
            if (admin == null)
            {
                Console.WriteLine("❌ Admin utilisateur non trouvé pour créer les posts.");
                return;
            }

            var posts = new List<Post>
            {
                new Post
                {
                    Id = Guid.NewGuid(),
                    CreatedBy = admin.Id,
                    Content = "🚀 Bienvenue sur RISE! Cette plateforme est dédiée à la gestion d'événements, sondages et élections au sein de notre communauté.",
                    IsPublic = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    UpdatedAt = DateTime.UtcNow.AddDays(-5),
                    ReactionCount = 12,
                    CommentCount = 3,
                    EventId = null
                },
                new Post
                {
                    Id = Guid.NewGuid(),
                    CreatedBy = admin.Id,
                    Content = "📢 Annonce: Un hackathon incroyable aura lieu le mois prochain! Préparez vos équipes et vos idées innovantes.",
                    IsPublic = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    UpdatedAt = DateTime.UtcNow.AddDays(-3),
                    ReactionCount = 25,
                    CommentCount = 8,
                    EventId = null
                },
                new Post
                {
                    Id = Guid.NewGuid(),
                    CreatedBy = admin.Id,
                    Content = "🎓 Les résultats du sondage sur les cours préférés sont maintenant disponibles! Consultez-les dans la section Sondages.",
                    IsPublic = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1),
                    ReactionCount = 8,
                    CommentCount = 2,
                    EventId = null
                },
                new Post
                {
                    Id = Guid.NewGuid(),
                    CreatedBy = admin.Id,
                    Content = "🗳️ Élections en cours! Votez pour vos candidats préférés pour le poste de représentant de classe. Le vote se termine demain à minuit.",
                    IsPublic = true,
                    CreatedAt = DateTime.UtcNow.AddHours(-2),
                    UpdatedAt = DateTime.UtcNow.AddHours(-2),
                    ReactionCount = 45,
                    CommentCount = 15,
                    EventId = null
                },
                new Post
                {
                    Id = Guid.NewGuid(),
                    CreatedBy = admin.Id,
                    Content = "🎉 Merci à tous les participants de la conférence d'hier! C'était un succès avec plus de 200 participants. Les slides sont disponibles à télécharger.",
                    IsPublic = true,
                    CreatedAt = DateTime.UtcNow.AddHours(-12),
                    UpdatedAt = DateTime.UtcNow.AddHours(-12),
                    ReactionCount = 32,
                    CommentCount = 5,
                    EventId = null
                }
            };

            context.Posts.AddRange(posts);
            context.SaveChanges();

            Console.WriteLine($"✅ {posts.Count} publications de test ont été créées avec succès!");
        }
    }
}
