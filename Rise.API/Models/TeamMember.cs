namespace Rise.API.Models
{
    public class TeamMember
    {
        public Guid Id { get; set; }
        public Guid AnswerId { get; set; }
        public Guid UserId { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        // Relationships
        public Answer Answer { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
