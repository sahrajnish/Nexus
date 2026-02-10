namespace Nexus.Identity.API.Features.VerifyOtp
{
    public sealed record class VerifyOtpResponse
    {
        public long UserId { get; init; }
        public string Email { get; init; } = default!;
        public bool IsVerified { get; init; }
        public string Message { get; init; } = default!;
    }
}
