using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client.Extensions.Msal;
using Nexus.Video.API.Data;
using Nexus.Video.API.Entities;

namespace Nexus.Video.API.Features.Videos.ConfirmUpload
{
    public class ConfirmUploadHandler : IRequestHandler<ConfirmUploadCommand, bool>
    {
        private readonly AppDbContext _dbContext;
        private readonly BlobServiceClient _blobServiceClient;

        public ConfirmUploadHandler(AppDbContext dbContext, BlobServiceClient blobServiceClient)
        {
            _dbContext = dbContext;
            _blobServiceClient = blobServiceClient;
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

            var containerClient = _blobServiceClient.GetBlobContainerClient(VideoConstants.RawVideoContainerName);
            var blobClient = containerClient.GetBlobClient($"{video.Id}{video.Extension}");

            if(!await blobClient.ExistsAsync(cancellationToken))
            {
                video.MarkAsFailed();
                await _dbContext.SaveChangesAsync(cancellationToken);
                return false;
            }

            // --- SECURITY: THE "MAGIC BYTES" CHECK ---
            // We never trust the file extension the user provided. 
            // To prevent downloading a massive 5GB file into server RAM just to validate it, 
            // we only download the first 256 bytes (the file header). This is instantaneous.
            using var stream = await blobClient.OpenReadAsync(new BlobOpenReadOptions(false) { BufferSize = 256 }, cancellationToken);
            var buffer = new byte[256];
            await stream.ReadExactlyAsync(buffer, 0, buffer.Length, cancellationToken);

            // Dispose stream
            stream.Dispose();

            // Convert those raw bytes into readable ASCII text to look for signatures.
            var headerText = System.Text.Encoding.ASCII.GetString(buffer);

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
                await blobClient.DeleteAsync(cancellationToken: cancellationToken);

                return false;
            }

            // Move video file from raw-videos to processing-videos.
            var processingContainer = _blobServiceClient.GetBlobContainerClient(VideoConstants.ProcessingVideoContainerName);

            // Create processing-videos storage if not exists.
            await processingContainer.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

            // Destination blob
            var destinationBlob = processingContainer.GetBlobClient($"{video.Id}{video.Extension}");

            // Start Copy process and wait for it to guarantee it finishes
            var copyOperation = await destinationBlob.StartCopyFromUriAsync(blobClient.Uri, cancellationToken: cancellationToken);
            await copyOperation.WaitForCompletionAsync(cancellationToken: cancellationToken);

            // Delete the original file from raw-videos storage
            await blobClient.DeleteAsync(cancellationToken: cancellationToken);

            // Mark as Processing in Db
            video.MarkAsProcessing();
            await _dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}   
