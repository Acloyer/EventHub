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
        public DbSet<FavoriteEvent> FavoriteEvents { get; set; }
        public DbSet<PlannedEvent>  PlannedEvents  { get; set; }
        public DbSet<TelegramVerification> TelegramVerifications { get; set; }

        // New features 
        public DbSet<EventComment>  EventComments   { get; set; }
        public DbSet<UserMuteEntry> UserMuteEntries { get; set; }
        public DbSet<Notification>  Notifications   { get; set; }
        // PostReactions
        public DbSet<PostReaction>  PostReactions   { get; set; }

        public EventHubDbContext(DbContextOptions<EventHubDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
<<<<<<< HEAD
                    
            // ==== TelegramVerification ====
            modelBuilder.Entity<TelegramVerification>(entity =>
            {
                entity.ToTable("TelegramVerifications");
                entity.HasKey(tv => tv.Id);
                entity.HasOne(tv => tv.User)
                    .WithMany(u => u.TelegramVerifications)
                    .HasForeignKey(tv => tv.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==== UserMuteEntry (one-to-one с User) ====
            modelBuilder.Entity<UserMuteEntry>(entity =>
            {
                entity.HasKey(m => m.UserId);
                entity.HasOne(m => m.User)
                    .WithOne()                  // у User не заводим ICollection<…>
                    .HasForeignKey<UserMuteEntry>(m => m.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==== Notifications ====
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasOne(n => n.User)
                    .WithMany()                // если в User нет ICollection<Notification>, иначе WithMany(u => u.Notifications)
                    .HasForeignKey(n => n.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==== EventComment ====
            modelBuilder.Entity<EventComment>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.HasOne(c => c.Event)
                    .WithMany(e => e.EventComments)
                    .HasForeignKey(c => c.EventId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.User)
                    .WithMany(u => u.EventComments)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(c => c.Comment)
                    .HasMaxLength(200)
                    .IsRequired();
            });


            modelBuilder.Entity<PostReaction>(entity =>
            {
                // 1) сообщаем, что свойство EventId хранится в столбце "PostId"
                entity.Property(r => r.EventId)
                    .HasColumnName("PostId");

                // 2) PK + уникальный индекс (одна реакция на одного юзера)
                entity.HasKey(r => r.Id);
                entity.HasIndex(r => new { r.EventId, r.UserId })
                    .IsUnique();

                // 3) FK на Events (PostId → Events.Id)
                entity.HasOne(r => r.Event)
                    .WithMany(e => e.PostReactions)
                    .HasForeignKey(r => r.EventId)
                    .OnDelete(DeleteBehavior.Cascade);

                // 4) FK на AspNetUsers
                entity.HasOne(r => r.User)
                    .WithMany(u => u.PostReactions)
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // 5) ограничения по Emoji
                entity.Property(r => r.Emoji)
                    .IsRequired()
                    .HasMaxLength(10);
            });

            // ==== Существующие сущности ====

=======

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
>>>>>>> 573f3e0705c1e3252b4cddd7cfc9446f4bee2932
            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired();
                entity.Property(e => e.Description).IsRequired();
                entity.HasOne(e => e.Creator)
<<<<<<< HEAD
                    .WithMany(u => u.CreatedEvents)
                    .HasForeignKey(e => e.CreatorId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<FavoriteEvent>(entity =>
            {
                entity.HasKey(f => new { f.UserId, f.EventId });
                entity.HasOne(f => f.User)
                    .WithMany(u => u.FavoriteEvents)
                    .HasForeignKey(f => f.UserId);
                entity.HasOne(f => f.Event)
                    .WithMany(e => e.FavoriteEvents)
                    .HasForeignKey(f => f.EventId);
            });

            modelBuilder.Entity<PlannedEvent>(entity =>
            {
                entity.HasKey(p => new { p.UserId, p.EventId });
                entity.HasOne(p => p.User)
                    .WithMany(u => u.PlannedEvents)
                    .HasForeignKey(p => p.UserId);
                entity.HasOne(p => p.Event)
                    .WithMany(e => e.PlannedEvents)
                    .HasForeignKey(p => p.EventId);
            });

            modelBuilder.Entity<User>()
                        .ToTable("AspNetUsers");
            modelBuilder.Entity<Role>()
                        .ToTable("AspNetRoles");
=======
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

>>>>>>> 573f3e0705c1e3252b4cddd7cfc9446f4bee2932
        }
    }
}