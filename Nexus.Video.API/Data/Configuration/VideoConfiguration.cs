using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoEntity = Nexus.Video.API.Entities.Video;

namespace Nexus.Video.API.Data.Configuration
{
    public class VideoConfiguration : IEntityTypeConfiguration<VideoEntity>
    {
        public void Configure(EntityTypeBuilder<VideoEntity> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.Title).HasMaxLength(255);
            builder.Property(x => x.Description).HasMaxLength(2000);
            builder.Property(x => x.ContentType).HasMaxLength(50);
            builder.Property(x => x.Extension).HasMaxLength(10);
        }
    }
}
