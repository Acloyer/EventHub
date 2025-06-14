using Microsoft.EntityFrameworkCore;
using EventHub.Models;
using System.Collections.Generic;

namespace EventHub.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<FavoriteEvent> FavoriteEvents { get; set; }
        public DbSet<PlannedEvent> PlannedEvents { get; set; }
        public DbSet<PostReaction> PostReactions { get; set; }
        public DbSet<TelegramVerification> TelegramVerifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Конфигурация User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Конфигурация Role
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Конфигурация UserRole
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UserId, e.RoleId }).IsUnique();

                entity.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Конфигурация Event
            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Creator)
                    .WithMany(u => u.CreatedEvents)
                    .HasForeignKey(e => e.CreatorId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Ignore(e => e.IsFavorite);
                entity.Ignore(e => e.IsPlanned);
            });

            // Конфигурация FavoriteEvent
            modelBuilder.Entity<FavoriteEvent>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.EventId });

                entity.HasOne(fe => fe.User)
                    .WithMany(u => u.FavoriteEvents)
                    .HasForeignKey(fe => fe.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(fe => fe.Event)
                    .WithMany(e => e.FavoriteEvents)
                    .HasForeignKey(fe => fe.EventId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Конфигурация PlannedEvent
            modelBuilder.Entity<PlannedEvent>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.EventId });

                entity.HasOne(pe => pe.User)
                    .WithMany(u => u.PlannedEvents)
                    .HasForeignKey(pe => pe.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pe => pe.Event)
                    .WithMany(e => e.PlannedEvents)
                    .HasForeignKey(pe => pe.EventId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PostReaction>()
                .HasKey(pr => pr.Id);

            modelBuilder.Entity<TelegramVerification>()
                .HasKey(tv => tv.Id);

            // Seed default roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin", UserRoles = new List<UserRole>() },
                new Role { Id = 2, Name = "User", UserRoles = new List<UserRole>() }
            );
        }
    }
} 