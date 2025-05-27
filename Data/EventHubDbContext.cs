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

        public EventHubDbContext(DbContextOptions opts) : base(opts) { }
        protected override void OnModelCreating(ModelBuilder model)
        {
            model.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            model.Entity<PostReaction>()
                .HasKey(pr => pr.Id);

            model.Entity<FavoriteEvent>()
                .HasKey(f => f.Id);

            model.Entity<FavoriteEvent>()
                .HasOne(f => f.User)
                .WithMany(u => u.FavoriteEvents)
                .HasForeignKey(f => f.UserId);

            model.Entity<FavoriteEvent>()
                .HasOne(f => f.Event)
                .WithMany()
                .HasForeignKey(f => f.EventId);

            model.Entity<PlannedEvent>()
                .HasKey(p => p.Id);

            model.Entity<PlannedEvent>()
                .HasOne(p => p.User)
                .WithMany(u => u.PlannedEvents)
                .HasForeignKey(p => p.UserId);

            model.Entity<PlannedEvent>()
                .HasOne(p => p.Event)
                .WithMany()
                .HasForeignKey(p => p.EventId);

            model.Entity<Event>()
                .HasOne(e => e.Organizer)
                .WithMany(u => u.CreatedEvents)
                .HasForeignKey(e => e.OrganizerId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(model);
        }
    }
}
