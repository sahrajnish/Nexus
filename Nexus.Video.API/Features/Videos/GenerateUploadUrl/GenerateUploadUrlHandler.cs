using Amazon.S3;
using Amazon.S3.Model;
using MediatR;
using Nexus.Shared.Utilities;
using Nexus.Video.API.Data;
using VideoEntity =  Nexus.Video.API.Entities.Video;

namespace Nexus.Video.API.Features.Videos.GenerateUploadUrl
{
    public class GenerateUploadUrlHandler : IRequestHandler<GenerateUploadUrlCommand, GenerateUploadUrlResponse>
    {
        private readonly IAmazonS3 _s3Client;
        private readonly SnowFlakeIdGenerator _idGenerator;
        private readonly AppDbContext _dbContext;
        public GenerateUploadUrlHandler(IAmazonS3 s3Client, SnowFlakeIdGenerator idGenerator, AppDbContext dbContext)
        {
            _s3Client = s3Client;
            _idGenerator = idGenerator;
            _dbContext = dbContext;
        }

        public async Task<GenerateUploadUrlResponse> Handle(GenerateUploadUrlCommand request, CancellationToken cancellationToken)
        {
            var videoId = _idGenerator.NextId();

            var extension = Path.GetExtension(request.FileName);
            var objectKey = $"{videoId}{extension}";

            var preSignedUrlRequest = new GetPreSignedUrlRequest
            {
                BucketName = VideoConstants.RawVideoContainerName,
                Key = objectKey,
                Verb = HttpVerb.PUT,
                Expires = DateTime.UtcNow.AddHours(VideoConstants.SasUrlExpirationHours),
                ContentType = request.ContentType
            };

            string preSignedUrl = _s3Client.GetPreSignedURL(preSignedUrlRequest);

            long dummyUserId = 1234567890; // Replace with actual user ID from authentication context

            var videoRecord = VideoEntity.Create(
                id: videoId,
                userId: dummyUserId,
                title: request.FileName,
                contentType: request.ContentType,
                extension: extension
            );

            _dbContext.Videos.Add(videoRecord); 
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new GenerateUploadUrlResponse(videoId.ToString(), preSignedUrl);
        }
    }
}
