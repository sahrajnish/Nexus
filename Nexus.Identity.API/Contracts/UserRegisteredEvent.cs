namespace Nexus.Identity.API.Contracts
{
    public record UserRegisteredEvent(
        string Email,
        string OtpCode
    );
}
