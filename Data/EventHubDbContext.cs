using EventHub.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Reflection.Emit;

namespace EventHub.Data
{ 
    public class EventHubDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<PostReaction> PostReactions { get; set; }
        public DbSet<FavoriteEvent> FavoriteEvents { get; set; }
        public DbSet<PlannedEvent> PlannedEvents { get; set; }
        public DbSet<TelegramVerification> TelegramVerifications { get; set; }

        public EventHubDbContext(DbContextOptions<EventHubDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Configure Role entity
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Configure UserRole entity
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId });

                entity.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId);

                entity.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId);
            });

            // Configure Event entity
            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired();
                entity.Property(e => e.Description).IsRequired();

                entity.HasOne(e => e.Creator)
                    .WithMany()
                    .HasForeignKey(e => e.CreatorId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PostReaction>()
                .HasKey(pr => pr.Id);

            // Configure FavoriteEvent entity
            modelBuilder.Entity<FavoriteEvent>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.EventId });

                entity.HasOne(f => f.User)
                    .WithMany(u => u.FavoriteEvents)
                    .HasForeignKey(f => f.UserId);

                entity.HasOne(f => f.Event)
                    .WithMany()
                    .HasForeignKey(f => f.EventId);
            });

            // Configure PlannedEvent entity
            modelBuilder.Entity<PlannedEvent>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.EventId });

                entity.HasOne(p => p.User)
                    .WithMany(u => u.PlannedEvents)
                    .HasForeignKey(p => p.UserId);

                entity.HasOne(p => p.Event)
                    .WithMany()
                    .HasForeignKey(p => p.EventId);
            });

            modelBuilder.Entity<TelegramVerification>()
                .HasKey(tv => tv.Id);
        }
    }
}
