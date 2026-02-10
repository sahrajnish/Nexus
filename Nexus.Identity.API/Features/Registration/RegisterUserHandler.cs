using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexus.Identity.API.Data;
using Nexus.Identity.API.Domain;
using Nexus.Shared.Utilities;
using Nexus.Identity.API.Domain.Exceptions;
using System.Data.Common;
using MassTransit;
using Nexus.Shared.Contracts.Identity;
using Nexus.Identity.API.Domain.Utils;

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
            _logger.LogInformation("Starting registration process for email {Email}", request.Email);

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(normalizedEmail))
            {
                throw new ConflictException(RegistrationConstants.ExceptionStrings.EmailValidationMessage);
            }

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
            if(existingUser != null)
            {
                _logger.LogWarning("Registration attempt with existing email {Email}", normalizedEmail);
                throw new ConflictException(RegistrationConstants.ExceptionStrings.EmailAlreadyExists);
            }

            var tempUser = await _context.TempUsers
                    .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
            if (tempUser != null && tempUser?.EmailExpiresAt > DateTime.UtcNow)
            {
                _logger.LogWarning("Registration attempt with email {Email} that has an active registration in progress", normalizedEmail);
                throw new ConflictException(RegistrationConstants.ExceptionStrings.RegistrationInProgress);
            }

            if(tempUser != null)
            {
                _context.TempUsers.Remove(tempUser);
            }

            string otpCode = OtpGenerator.GenerateSecureOtp();
            var otpToSend = otpCode;
            otpCode = BCrypt.Net.BCrypt.HashPassword(otpCode);

            var newTempUser = new TempUser
            {
                Id = _idGenerator.NextId(),
                Email = normalizedEmail,
                EmailExpiresAt = DateTime.UtcNow
                    .AddMinutes(RegistrationConstants.EmailVerificationExpiryMinutes),
                UpdatedAt = DateTime.UtcNow
            };

            var registerOtp = new Otp
            {
                Id = _idGenerator.NextId(),
                Email = normalizedEmail,
                Code = otpCode,
                Purpose = OtpPurpose.Register,
                ExpiresAt = DateTime.UtcNow.AddMinutes(RegistrationConstants.OtpExpiryMinutes),
                UpdatedAt = DateTime.UtcNow
            };


            await _context.TempUsers.AddAsync(newTempUser, cancellationToken);
            await _context.Otps.AddAsync(registerOtp, cancellationToken);

            await _publishEndpoint.Publish(new UserRegisteredEvent
            {
                Email = normalizedEmail,
                OtpCode = otpToSend
            }, cancellationToken);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("User {Email} saved to Table.", normalizedEmail);
                _logger.LogInformation("Published OTP for email {Email}", normalizedEmail);
            }
            catch (DbException ex)
            {
                _logger.LogError("Database error occurred while saving TempUser for email {Email}", normalizedEmail);
                _logger.LogError(ex.Message);
                throw new ConflictException(RegistrationConstants.ExceptionStrings.EmailAlreadyExists);
            }

            return newTempUser.Id;
        }
    }
}
