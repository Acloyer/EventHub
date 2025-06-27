using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EventHub.Models;

namespace EventHub.Data
{
    public class EventHubDbContext : IdentityDbContext<User, Role, int>
    {
        public DbSet<Role> Roles { get; set; }
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

            modelBuilder.Entity<TelegramVerification>()
                .HasOne(tv => tv.User)
                .WithMany()
                .HasForeignKey(tv => tv.UserId);

            // Вызов базового
            base.OnModelCreating(modelBuilder);

            // Переименование таблицы пользователя на AspNetUsers
            modelBuilder.Entity<User>()
                .ToTable("AspNetUsers");

            // Конфигурация Role (IdentityRole настроен в IdentityDbContext)
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("AspNetRoles");
            });

            // Конфигурация Event
            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired();
                entity.Property(e => e.Description).IsRequired();

                entity.HasOne(e => e.Creator)
                      .WithMany(u => u.CreatedEvents)
                      .HasForeignKey(e => e.CreatorId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Конфигурация PostReaction
            modelBuilder.Entity<PostReaction>()
                .HasKey(pr => pr.Id);

            // Конфигурация FavoriteEvent
            modelBuilder.Entity<FavoriteEvent>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.EventId });
                entity.HasOne(f => f.User)
                      .WithMany(u => u.FavoriteEvents)
                      .HasForeignKey(f => f.UserId);
                entity.HasOne(f => f.Event)
                      .WithMany(e => e.FavoriteEvents)
                      .HasForeignKey(f => f.EventId);
            });

            // Конфигурация PlannedEvent
            modelBuilder.Entity<PlannedEvent>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.EventId });
                entity.HasOne(p => p.User)
                      .WithMany(u => u.PlannedEvents)
                      .HasForeignKey(p => p.UserId);
                entity.HasOne(p => p.Event)
                      .WithMany(e => e.PlannedEvents)
                      .HasForeignKey(p => p.EventId);
            });

            // Конфигурация TelegramVerification
            modelBuilder.Entity<TelegramVerification>()
                .HasKey(tv => tv.Id);

            modelBuilder.Entity<TelegramVerification>(entity =>
            {
                entity.ToTable("TelegramVerifications");
                entity.HasKey(tv => tv.Id);

                entity
                .HasOne(tv => tv.User)
                .WithMany(u => u.TelegramVerifications)
                .HasForeignKey(tv => tv.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            });

        }
    }
}