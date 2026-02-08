using Nexus.Identity.API.Domain.Utils;

namespace Nexus.Identity.API.Domain
{
    public class Otp
    {
        public long Id { get; set; }
        public string Email { get; set; } = default!;
        public string Code { get; set; } = default!;
        public OtpPurpose Purpose { get; set; }
        public bool IsUsed { get; set; }
        public int Attempts { get; set; }
        public int ResendAttempts { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? LastAttemptAt { get; set; }
        public DateTime? LastResendAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Otp()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = CreatedAt;

            IsUsed = false;

            Attempts = 0;
            ResendAttempts = 0;
        }
    }
}
