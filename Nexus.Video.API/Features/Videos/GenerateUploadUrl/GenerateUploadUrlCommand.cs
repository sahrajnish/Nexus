using MediatR;

namespace Nexus.Video.API.Features.Videos.GenerateUploadUrl
{
    public record GenerateUploadUrlCommand(string FileName, string ContentType) : IRequest<GenerateUploadUrlResponse>;

    public record GenerateUploadUrlResponse(string VideoId, string UploadUrl);
}
