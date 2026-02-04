using MediatR;

namespace Nexus.Identity.API.Features.Registration
{
    public record class RegisterUserCommand(
        string Email
    ) : IRequest<long>;
}
