using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexus.Identity.API.Data;
using Nexus.Identity.API.Domain;
using Nexus.Shared.Utilities;
using Nexus.Identity.API.Constants;
using Nexus.Identity.API.Domain.Exceptions;
using System.Data.Common;

namespace Nexus.Identity.API.Features.Registration
{
    public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, long>
    {
        private readonly AppDbContext _context;
        private readonly SnowFlakeIdGenerator _idGenerator;
        public RegisterUserHandler(AppDbContext context, SnowFlakeIdGenerator idGenerator)
        {
            _context = context;
            _idGenerator = idGenerator;
        }

        public async Task<long> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var existingUser = await _context.Users
                .AnyAsync(u => u.Email == request.Email, cancellationToken);
            if(existingUser)
            {
                throw new ConflictException("Email already exists.");
            }

            var tempUser = await _context.TempUsers
                    .FirstOrDefaultAsync(u => u.Email == request.Email);
            if (tempUser != null && tempUser?.EmailExpiresAt > DateTime.UtcNow)
            {
                throw new ConflictException("A registration process is already in progress for this email.");
            }

            if(tempUser != null)
            {
                _context.TempUsers.Remove(tempUser);
            }

            var newTempUser = new TempUser
            {
                Id = _idGenerator.NextId(),
                Email = request.Email,
                EmailExpiresAt = DateTime.UtcNow
                .AddMinutes(RegistrationConstants.EmailVerificationExpiryMinutes)
            };

            await _context.TempUsers.AddAsync(newTempUser, cancellationToken);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbException ex)
            {
                throw new ConflictException("Email already exists.");
            }

            return newTempUser.Id;
        }
    }
}
