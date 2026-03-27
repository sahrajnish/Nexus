using Amazon.S3;
using Amazon.S3.Model;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexus.Video.API.Data;
using Nexus.Video.API.Entities;
using System.Net;

namespace Nexus.Video.API.Features.Videos.ConfirmUpload
{
    public class ConfirmUploadHandler : IRequestHandler<ConfirmUploadCommand, bool>
    {
        private readonly AppDbContext _dbContext;
        private readonly IAmazonS3 _s3Client;

        public ConfirmUploadHandler(AppDbContext dbContext, IAmazonS3 s3Client)
        {
            _dbContext = dbContext;
            _s3Client = s3Client;
        }

        public async Task<bool> Handle(ConfirmUploadCommand request, CancellationToken cancellationToken)
        {
            if(request.VideoId <= 0)
            {
                throw new ArgumentException("Invalid video ID.", nameof(request.VideoId));
            }

            var video = await _dbContext.Videos
                .FirstOrDefaultAsync(v => v.Id == request.VideoId, cancellationToken);

            if(video == null || video.Status != VideoStatus.Uploading)
            {
                return false;
            }

            var objectKey = $"{video.Id}{video.Extension}";

            try
            {
                await _s3Client.GetObjectMetadataAsync(VideoConstants.RawVideoContainerName, objectKey, cancellationToken);
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                video.MarkAsFailed();
                await _dbContext.SaveChangesAsync(cancellationToken);
                return false;
            }

            var getObjectRequest = new GetObjectRequest
            {
                BucketName = VideoConstants.RawVideoContainerName,
                Key = objectKey,
                ByteRange = new ByteRange(0, 255) // Read only the first 256 bytes for "magic bytes" validation
            };

            // --- SECURITY: THE "MAGIC BYTES" CHECK ---
            // We never trust the file extension the user provided. 
            // To prevent downloading a massive 5GB file into server RAM just to validate it, 
            // we only download the first 256 bytes (the file header). This is instantaneous.
            using var response = await _s3Client.GetObjectAsync(getObjectRequest, cancellationToken);
            using var stream = response.ResponseStream;

            var buffer = new byte[256];

            // Using ReadAsync instead of ReadExactlyAsync just in case someone uploaded a 10-byte text file
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

            // Convert those raw bytes into readable ASCII text to look for signatures.
            var headerText = System.Text.Encoding.ASCII.GetString(buffer, 0, bytesRead);

            // Real video files always stamp their format deep inside their binary headers.
            // "ftyp"     -> Standard signature for MP4 / QuickTime files
            // "webm"     -> Standard signature for WebM files
            // "matroska" -> Standard signature for MKV files
            if (!headerText.Contains("ftyp") && !headerText.Contains("webm") && !headerText.Contains("matroska"))
            {
                // Someone tried to upload a non-video file (like a .png or .exe) disguised as an .mp4.

                // Update the database so the user's UI shows a failed status.
                video.MarkAsFailed();
                await _dbContext.SaveChangesAsync(cancellationToken);

                // Destroy the malicious / invalid file from the raw vault immediately to save storage.
                await _s3Client.DeleteObjectAsync(VideoConstants.RawVideoContainerName, objectKey, cancellationToken);
                return false;
            }

            // Copy video file from raw-videos to processing-videos.
            var copyRequest = new CopyObjectRequest
            {
                SourceBucket = VideoConstants.RawVideoContainerName,
                SourceKey = objectKey,
                DestinationBucket = VideoConstants.ProcessingVideoContainerName,
                DestinationKey = objectKey
            };

            // This executes completely on the Cloudflare/MinIO servers. It doesn't download to your API.
            await _s3Client.CopyObjectAsync(copyRequest, cancellationToken);

            // Delete the original file from raw-videos storage
            await _s3Client.DeleteObjectAsync(VideoConstants.RawVideoContainerName, objectKey, cancellationToken);

            // Mark as Processing in Db
            video.MarkAsProcessing();
            await _dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}   
