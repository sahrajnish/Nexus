namespace Nexus.Video.API.Entities
{
    public class Video
    {
        public long Id { get; private set; }
        public long UserId { get; private set; }
        public string Title { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public string ContentType { get; private set; } = string.Empty;
        public string Extension { get; private set; } = string.Empty;
        public VideoStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        private Video() { }

        public static Video Create(long id, long userId, string title, string contentType, string extension)
        {
            return new Video
            {
                Id = id,
                UserId = userId,
                Title = title,
                ContentType = contentType,
                Extension = extension,
                Status = VideoStatus.Uploading,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void MarkAsUploadCompleted()
        {
            Status = VideoStatus.UploadCompleted;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsProcessing()
        {
            Status = VideoStatus.Processing;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsPublished()
        {
            Status = VideoStatus.Published;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsFailed()
        {
            Status = VideoStatus.Failed;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
