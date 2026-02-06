namespace Nexus.Shared.Contracts.Identity
{
    public record UserRegisteredEvent(
        string Email,
        string OtpCode
    );
}
