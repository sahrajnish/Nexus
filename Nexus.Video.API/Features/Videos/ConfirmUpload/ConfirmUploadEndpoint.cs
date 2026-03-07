using Carter;
using MediatR;

namespace Nexus.Video.API.Features.Videos.ConfirmUpload
{
    public class ConfirmUploadEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/videos/{id}/confirm", async (string id, ISender sender) =>
            {
                if(!long.TryParse(id, out var videoId))
                {
                    return Results.BadRequest(new { Message = "Invalid Video ID format." });
                }

                var result = await sender.Send(new ConfirmUploadCommand(videoId));

                if (!result)
                {
                    return Results.BadRequest(new
                    {
                        Message = "Upload confirmation failed. File may be missing, invalid, or already processed."
                    });
                }

                return Results.Ok(new
                {
                    Message = "Upload confirmed. File is valid and is now processing."
                });
            })
            .WithName("ConfirmUpload")
            .WithTags("Videos");
        }
    }
}
