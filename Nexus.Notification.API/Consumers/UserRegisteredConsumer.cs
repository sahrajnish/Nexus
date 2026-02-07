using MassTransit;
using Nexus.Notification.API.Services;
using Nexus.Shared.Contracts.Identity;

namespace Nexus.Notification.API.Consumers
{
    public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<UserRegisteredConsumer> _logger;

        public UserRegisteredConsumer(IEmailService emailService, ILogger<UserRegisteredConsumer> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
        {
            _logger.LogInformation("Received OTP in Notification Service for email: {Email}", context.Message.Email);
            var message = context.Message;

            await _emailService.SendEmailAsync(message.Email, message.OtpCode);
        }
    }
}
