
using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Nexus.Notification.API.Configurations;
using Nexus.Notification.API.Constants;

namespace Nexus.Notification.API.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly MailjetOptions _mailjetOptions;
        private readonly MailjetClient _mailjetClient;
        public EmailService(ILogger<EmailService> logger, IOptions<MailjetOptions> mailjetOptions)
        {
            _logger = logger;
            _mailjetOptions = mailjetOptions.Value;

            _mailjetClient = new MailjetClient(
                _mailjetOptions.ApiKey,
                _mailjetOptions.ApiSecret
            );
        }

        public async Task SendEmailAsync(string email, string otpCode)
        {
            try
            {
                var request = new MailjetRequest
                {
                    Resource = SendV31.Resource
                }
                .Property(Send.Messages, new JArray
                {
                    new JObject
                    {
                        {
                            "From",
                            new JObject
                            {
                                { "Email", _mailjetOptions.FromEmail },
                                { "Name", _mailjetOptions.FromName }
                            }
                        },
                        {
                            "To",
                            new JArray
                            {
                                new JObject
                                {
                                    { "Email", email }
                                }
                            }
                        },
                        {
                            "Subject", OtpEmailConstants.Subject
                        },
                        {
                            "TextPart",
                            $"{OtpEmailConstants.EmailTemplateText
                                .Replace("{0}", otpCode)
                                .Replace("{1}", OtpEmailConstants.OtpExpiryMinutes.ToString())}"
                        },
                        {
                            "HTMLPart",
                            $"{OtpEmailConstants.EmailTemplateHTML
                                .Replace("{0}", otpCode)
                                .Replace("{1}", OtpEmailConstants.OtpExpiryMinutes.ToString())}"
                        }
                    }
                });

                var response = await _mailjetClient.PostAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("OTP successfully sent to {Email}", email);
                }
                else
                {
                    _logger.LogError("Mailjet failed. StatusCode: {StatusCode}, Response: {Response}", response.StatusCode, response.GetData());

                    throw new Exception("Failed to send email via Mailjet");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error sending email to {Email}: {Message}", email, ex.Message);
                throw;
            }
        }
    }
}
