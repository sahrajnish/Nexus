namespace Nexus.Identity.API.Domain
{
    public class User
    {
        public long Id { get; init; }
        public string Email { get; set; } = default!;
        public string NormalizedEmail { get; set; } = default!;
        public string? PasswordHash { get; set; }
        public string? FullName { get; set; }
        public bool IsEmailVerified { get; set; }
        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LockoutEndAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime? PasswordChangedAt { get; set; }

        // Used to invalidate tokens when password changes or other security-related events occur
        public string SecurityStamp { get; set; } = Guid.NewGuid().ToString(); 
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // Support for EF Core
        // EF Core can use to User instance.
        // private User() { }
    }
}
