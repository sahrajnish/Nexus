using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using MediatR;
using Nexus.Shared.Utilities;
using Nexus.Video.API.Data;
using VideoEntity =  Nexus.Video.API.Entities.Video;

namespace Nexus.Video.API.Features.Videos.GenerateUploadUrl
{
    public class GenerateUploadUrlHandler : IRequestHandler<GenerateUploadUrlCommand, GenerateUploadUrlResponse>
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly SnowFlakeIdGenerator _idGenerator;
        private readonly AppDbContext _dbContext;
        public GenerateUploadUrlHandler(BlobServiceClient blobServiceClient, SnowFlakeIdGenerator idGenerator, AppDbContext dbContext)
        {
            _blobServiceClient = blobServiceClient;
            _idGenerator = idGenerator;
            _dbContext = dbContext;
        }

        public async Task<GenerateUploadUrlResponse> Handle(GenerateUploadUrlCommand request, CancellationToken cancellationToken)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(VideoConstants.RawVideoContainerName);
            await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

            var videoId = _idGenerator.NextId();

            var extension = Path.GetExtension(request.FileName);
            var blobName = $"{videoId}{extension}";

            var blobClient = containerClient.GetBlobClient(blobName);

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerClient.Name,
                BlobName = blobClient.Name,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(VideoConstants.SasUrlExpirationHours)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Write | BlobSasPermissions.Create);
            var sasUri = blobClient.GenerateSasUri(sasBuilder);

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

            return new GenerateUploadUrlResponse(videoId.ToString(), sasUri.ToString());
        }
    }
}
