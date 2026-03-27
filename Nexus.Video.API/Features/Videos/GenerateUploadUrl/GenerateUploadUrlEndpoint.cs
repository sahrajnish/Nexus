using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Nexus.Video.API.Features.Videos.GenerateUploadUrl
{
    public class GenerateUploadUrlEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/videos/upload-url", async (
                [FromBody] GenerateUploadUrlCommand command,
                IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return Results.Ok(result);
            })
            .WithName(nameof(GenerateUploadUrlCommand))
            .WithTags("Videos");
        }
    }
}
