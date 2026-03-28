using System;
using System.Collections.Generic;
using System.Text;

namespace Nexus.Shared.Contracts.Video
{
    public record VideoUploadConfirmedEvent
    {
        public long VideoId { get; init; }
        public long UserId { get; init; }
        public string OriginalFileName { get; init; } = default!;
        public string ContentType { get; init; } = default!;
        public string StoragePath { get; init; } = default!;
    }
}
