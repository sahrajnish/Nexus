using Microsoft.EntityFrameworkCore;
using Nexus.Identity.API.Data;
using Nexus.Identity.API.Domain;
using Nexus.Identity.API.Domain.Exceptions;
using Nexus.Identity.API.Domain.Utils;

namespace Nexus.Identity.API.Services
{
    public class OtpVerificationService
    {
        private readonly AppDbContext _dbContext;
        public OtpVerificationService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Otp> VerifyAsync(string email, string otp, OtpPurpose purpose, CancellationToken cancellationToken)
        { 
            var otpInDb = await _dbContext.Otps
                .Where(
                    x => x.Email == email &&
                    x.Purpose == purpose &&
                    !x.IsUsed)
                .FirstOrDefaultAsync(cancellationToken);
            if(otpInDb == null)
            {
                throw new NotFoundException("OTP not found.");
            }

            if(otpInDb.ExpiresAt < DateTime.UtcNow)
            {
                throw new Exception("OTP has expired.");
            }

            if(otpInDb.Attempts >= 3)
            {
                throw new Exception("OTP has been used too many times.");
            }

            var isMatch = BCrypt.Net.BCrypt.Verify(otp, otpInDb.Code);
            if(!isMatch)
            {
                otpInDb.Attempts++;
                otpInDb.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync(cancellationToken);
                throw new Exception("OTP is incorrect.");
            }

            otpInDb.IsUsed = true;
            otpInDb.UpdatedAt = DateTime.UtcNow;

            return otpInDb;
        }
    }
}
