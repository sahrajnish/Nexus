using FluentValidation;

namespace Nexus.Video.API.Features.Videos.GenerateUploadUrl
{
    public class GenerateUploadUrlValidator : AbstractValidator<GenerateUploadUrlCommand>
    {
        public GenerateUploadUrlValidator()
        {
            RuleFor(x => x.FileName)
                .NotEmpty()
                .WithMessage("File name is required.");

            RuleFor(x => x.ContentType)
                .NotEmpty()
                .WithMessage("Content type is required.")
                .Must(contentType => contentType.StartsWith("video/"))
                .WithMessage("Only video files are allowed.");
        }
    }
}
