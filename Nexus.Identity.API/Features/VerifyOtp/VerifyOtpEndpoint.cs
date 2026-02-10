using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Nexus.Identity.API.Features.VerifyOtp
{
    public static class VerifyOtpEndpoint
    {
        public static void MapVerifyOtpEndPoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/auth/verify-otp", async (
                [FromBody] VerifyOtpCommand command,
                IMediator mediator,
                IValidator<VerifyOtpCommand> validator,
                CancellationToken cancellationToken) => 
            { 
                var validatorResult = await validator.ValidateAsync(command);
                if (!validatorResult.IsValid)
                {
                    return Results.ValidationProblem(validatorResult.ToDictionary());
                }

                var response = await mediator.Send(command, cancellationToken);

                return Results.Created($"/api/users/{response.UserId}", response);
            })
                .WithName("VerifyOtp")
                .WithTags("Auth")
                .Produces<VerifyOtpResponse>(StatusCodes.Status200OK)
                .ProducesValidationProblem()
                .Produces(StatusCodes.Status400BadRequest);
        }
    }
}
