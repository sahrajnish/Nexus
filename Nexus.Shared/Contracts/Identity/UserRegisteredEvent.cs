namespace Nexus.Shared.Contracts.Identity
{
    public record UserRegisteredEvent
    {
        public string Email { get; init; } = default!;
        public string OtpCode { get; init; } = default!;
    };
}
