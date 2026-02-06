namespace Nexus.Notification.API.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string otpCode);
    }
}
