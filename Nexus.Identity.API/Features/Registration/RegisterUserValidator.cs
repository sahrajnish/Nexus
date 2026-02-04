using FluentValidation;

namespace Nexus.Identity.API.Features.Registration
{
    public class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .WithMessage("A valid email address is required.");
        }
    }
}
