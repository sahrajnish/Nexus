using Nexus.Identity.API.Features.Registration;

namespace Nexus.Identity.API.Domain
{
    public class TempUser
    {
        public long Id { get; set; }
        public string Email { get; set; } = default!;
        public DateTime EmailExpiresAt { get; set; }
        public DateTime EmailAddedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public TempUser()
        {
            EmailAddedAt = DateTime.UtcNow;
            EmailExpiresAt = EmailAddedAt.AddMinutes(RegistrationConstants.EmailVerificationExpiryMinutes);
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = CreatedAt;
        }
    }
}
