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
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255);

            modelBuilder.Entity<TempUser>(entity =>
            {
                entity.HasKey(tu => tu.Id);

                entity.HasIndex(tu => tu.Email).IsUnique();

                entity.Property(tu => tu.Email).IsRequired().HasMaxLength(255);
            });

            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();
        }
    }
}
