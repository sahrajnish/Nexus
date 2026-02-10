using MassTransit;
using Microsoft.EntityFrameworkCore;
using Nexus.Identity.API.Domain;

namespace Nexus.Identity.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }

        public DbSet<User> Users { get; set; }
        public DbSet<TempUser> TempUsers { get; set; }
        public DbSet<Otp> Otps { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.NormalizedEmail).IsRequired().HasMaxLength(255);
                entity.HasIndex(u => u.NormalizedEmail).IsUnique();
                entity.Property(u => u.IsEmailVerified).IsRequired();
                entity.Property(u => u.IsDeleted).IsRequired();
                entity.Property(u => u.FailedLoginAttempts).IsRequired();
                entity.Property(u => u.PasswordHash).HasMaxLength(500);
                entity.Property(u => u.FullName).HasMaxLength(200);
                entity.Property(u => u.SecurityStamp).IsRequired().HasMaxLength(200);
                entity.Property(u => u.CreatedAt).IsRequired();
                entity.Property(u => u.UpdatedAt).IsRequired();

                entity.HasIndex(u => new { u.NormalizedEmail, u.DeletedAt });
            });

            modelBuilder.Entity<TempUser>(entity =>
            {
                entity.HasKey(tu => tu.Id);
                entity.HasIndex(tu => tu.Email).IsUnique();
                entity.Property(tu => tu.Email).IsRequired().HasMaxLength(255);
            });

            modelBuilder.Entity<Otp>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.HasIndex(x => new { x.Email, x.Purpose, x.IsUsed });
                entity.Property(o => o.Email).IsRequired().HasMaxLength(255);
                entity.Property(o => o.Code).IsRequired().HasMaxLength(255);
                entity.Property(o => o.Purpose).IsRequired();
                entity.Property(o => o.IsUsed).IsRequired();
                entity.Property(o => o.Attempts).IsRequired();
                entity.Property(o => o.ResendAttempts).IsRequired();
                entity.Property(o => o.CreatedAt).IsRequired();
                entity.Property(o => o.ExpiresAt).IsRequired();
                entity.Property(o => o.UpdatedAt).IsRequired();
            });

            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();
        }
    }
}
