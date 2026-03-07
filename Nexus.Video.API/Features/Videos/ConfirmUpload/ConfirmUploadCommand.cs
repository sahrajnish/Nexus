using MediatR;

namespace Nexus.Video.API.Features.Videos.ConfirmUpload
{
    public record ConfirmUploadCommand(long VideoId) : IRequest<bool>;
}
