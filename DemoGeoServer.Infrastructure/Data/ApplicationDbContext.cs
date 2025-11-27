using DemoGeoServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DemoGeoServer.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                .HasColumnName("id")
                .UseIdentityAlwaysColumn();

                entity.Property(e => e.Username)
                .HasColumnName("username")
                .HasMaxLength(100)
                .IsRequired();

                entity.Property(e => e.Email)
                .HasColumnName("email")
                .HasMaxLength(255)
                .IsRequired();

                entity.Property(e => e.PasswordHash)
                .HasColumnName("password_hash")
                .HasMaxLength(255);

                entity.Property(e => e.Role)
                .HasColumnName("role")
                .HasMaxLength(50);

                entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()")
                .IsRequired();
            });

            // RefreshToken configuration
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("refresh_tokens");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                .HasColumnName("id")
                .UseIdentityAlwaysColumn();

                entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();

                entity.Property(e => e.Token)
                .HasColumnName("token")
                .HasColumnType("text")
                .IsRequired();

                entity.Property(e => e.ExpiryDate)
                .HasColumnName("expiry_date")
                .IsRequired();

                entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()");

                entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("now()");

                // Foreign key relationship
                entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
