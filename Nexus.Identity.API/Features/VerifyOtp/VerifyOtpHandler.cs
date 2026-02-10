using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexus.Identity.API.Data;
using Nexus.Identity.API.Domain;
using Nexus.Identity.API.Domain.Exceptions;
using Nexus.Identity.API.Domain.Utils;
using Nexus.Identity.API.Services;
using Nexus.Shared.Utilities;

namespace Nexus.Identity.API.Features.VerifyOtp
{
    public class VerifyOtpHandler : IRequestHandler<VerifyOtpCommand, VerifyOtpResponse>
    {
        private readonly ILogger<VerifyOtpHandler> _logger;
        private readonly AppDbContext _dbContext;
        private readonly SnowFlakeIdGenerator _idGenerator;
        private readonly OtpVerificationService _otpVerificationService;
        public VerifyOtpHandler(AppDbContext dbContext, ILogger<VerifyOtpHandler> logger, SnowFlakeIdGenerator idGenerator, OtpVerificationService otpVerificationService)
        {
            _dbContext = dbContext;
            _logger = logger;
            _idGenerator = idGenerator;
            _otpVerificationService = otpVerificationService;
        }

        public async Task<VerifyOtpResponse> Handle(
            VerifyOtpCommand request,
            CancellationToken cancellationToken)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            if (string.IsNullOrEmpty(normalizedEmail))
                throw new BadRequestException("Email cannot be empty.");

            var strategy = _dbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction =
                    await _dbContext.Database.BeginTransactionAsync(cancellationToken);

                await _otpVerificationService.VerifyAsync(
                    normalizedEmail,
                    request.Otp,
                    request.Purpose,
                    cancellationToken);

                VerifyOtpResponse response = request.Purpose switch
                {
                    OtpPurpose.Register =>
                        await HandleRegistrationAsync(normalizedEmail, cancellationToken),

                    _ => throw new BadRequestException("Invalid OTP purpose.")
                };

                await _dbContext.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return response;
            });
        }


        private async Task<VerifyOtpResponse> HandleRegistrationAsync(string email, CancellationToken cancellationToken)
        {
            var tempUser = await _dbContext.TempUsers.FirstOrDefaultAsync(tu => tu.Email == email, cancellationToken);
            if (tempUser == null)
            {
                throw new NotFoundException("Registration not initiated.");
            }

            var user = new User
            {
                Id = _idGenerator.NextId(),
                Email = email,
                NormalizedEmail = email.ToLowerInvariant(),
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow
            };

            await _dbContext.Users.AddAsync(user, cancellationToken);
            _dbContext.TempUsers.Remove(tempUser);

            return new VerifyOtpResponse
            {
                UserId = user.Id,
                Email = user.Email,
                IsVerified = true,
                Message = "Registration successful. Email verified."
            };
        }
    }
}
