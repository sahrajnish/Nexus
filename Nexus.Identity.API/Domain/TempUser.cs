using Nexus.Identity.API.Features.Registration;

namespace Nexus.Identity.API.Domain
{
    public class TempUser
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public DateTime EmailExpiresAt { get; set; }
        public DateTime EmailAddedAt { get; set; }
        public string? OtpCode { get; set; }
        public DateTime? OtpCreatedAt { get; set; }
        public DateTime? OtpExpiresAt { get; set; }
        public bool IsOtpUsed { get; set; }
        public int OtpAttempts { get; set; }
        public int OtpResendAttempts { get; set; }
        public DateTime? OtpReattemptAt { get; set; }
        public DateTime? ResendReattemptAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public TempUser()
        {
            EmailAddedAt = DateTime.UtcNow;
            EmailExpiresAt = EmailAddedAt.AddMinutes(RegistrationConstants.EmailVerificationExpiryMinutes);
            CreatedAt = DateTime.UtcNow;
            IsOtpUsed = false;
            OtpAttempts = 0;
            OtpResendAttempts = 0;
            OtpReattemptAt = null;
            ResendReattemptAt = null;
            UpdatedAt = CreatedAt;
        }
    }
}
