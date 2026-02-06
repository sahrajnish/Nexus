using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexus.Identity.API.Data;
using Nexus.Identity.API.Domain;
using Nexus.Shared.Utilities;
using Nexus.Identity.API.Domain.Exceptions;
using System.Data.Common;
using MassTransit;
using Nexus.Shared.Contracts.Identity;

namespace Nexus.Identity.API.Features.Registration
{
    public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, long>
    {
        private readonly AppDbContext _context;
        private readonly SnowFlakeIdGenerator _idGenerator;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<RegisterUserHandler> _logger;
        public RegisterUserHandler(AppDbContext context, SnowFlakeIdGenerator idGenerator, IPublishEndpoint publishEndpoint, ILogger<RegisterUserHandler> logger)
        {
            _context = context;
            _idGenerator = idGenerator;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task<long> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        { 
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(normalizedEmail))
            {
                throw new ConflictException(RegistrationConstants.ExceptionStrings.EmailValidationMessage);
            }

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
            if(existingUser != null)
            {
                throw new ConflictException(RegistrationConstants.ExceptionStrings.EmailAlreadyExists);
            }

            var tempUser = await _context.TempUsers
                    .FirstOrDefaultAsync(u => u.Email == normalizedEmail);
            if (tempUser != null && tempUser?.EmailExpiresAt > DateTime.UtcNow)
            {
                throw new ConflictException(RegistrationConstants.ExceptionStrings.RegistrationInProgress);
            }

            if(tempUser != null)
            {
                _context.TempUsers.Remove(tempUser);
            }

            string OtpCode = OtpGenerator.GenerateSecureOtp();

            var newTempUser = new TempUser
            {
                Id = _idGenerator.NextId(),
                Email = normalizedEmail,
                EmailExpiresAt = DateTime.UtcNow
                    .AddMinutes(RegistrationConstants.EmailVerificationExpiryMinutes),
                OtpCode = OtpCode,
                OtpAttempts = 1,
                OtpCreatedAt = DateTime.UtcNow,
                OtpExpiresAt = DateTime.UtcNow
                    .AddMinutes(RegistrationConstants.OtpExpiryMinutes),
            };

            await _context.TempUsers.AddAsync(newTempUser, cancellationToken);

            await _publishEndpoint.Publish(new UserRegisteredEvent(normalizedEmail, OtpCode), cancellationToken);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Otp {OtpCode} generated for email {Email}", OtpCode, normalizedEmail);
            }
            catch (DbException ex)
            {
                throw new ConflictException(RegistrationConstants.ExceptionStrings.EmailAlreadyExists);
            }

            return newTempUser.Id;
        }
    }
}
