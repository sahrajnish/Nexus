using FluentValidation;

namespace Nexus.Identity.API.Features.VerifyOtp
{
    public class VerifyOtpValidator : AbstractValidator<VerifyOtpCommand>
    {
        public VerifyOtpValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .WithMessage(VerifyOtpConstants.ExceptionStrings.EmailValidationMessage);

            RuleFor(x => x.Otp)
                .NotEmpty()
                .Matches(@"^\d{6}$")
                .WithMessage(VerifyOtpConstants.ExceptionStrings.OtpValidationMessage);

            RuleFor(x => x.Purpose)
                .IsInEnum()
                .WithMessage("Invalid OTP purpose.");
        }
    }
}
