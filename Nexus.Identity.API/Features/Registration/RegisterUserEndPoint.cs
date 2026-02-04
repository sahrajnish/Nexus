using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Nexus.Identity.API.Features.Registration
{
    public static class RegisterUserEndPoint
    {
        public static void MapRegisterUserEndPoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/users/register", async (
                [FromBody] RegisterUserCommand command,
                IMediator mediator,
                IValidator<RegisterUserCommand> validator) =>
            {
                var validatorResult = await validator.ValidateAsync(command);
                if(!validatorResult.IsValid)
                {
                    return Results.ValidationProblem(validatorResult.ToDictionary());
                }

                var userId = await mediator.Send(command);
                return Results.Created($"/api/users/{userId}", new { Id = userId });
            })
            .WithName("RegisterUser");
        }
    }
}
