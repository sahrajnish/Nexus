using MediatR;
using Nexus.Identity.API.Domain.Utils;

namespace Nexus.Identity.API.Features.VerifyOtp
{
    public sealed record class VerifyOtpCommand(
        string Email,
        string Otp,
        OtpPurpose Purpose
    ) : IRequest<VerifyOtpResponse>;
}
