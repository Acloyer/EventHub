using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EventHub.Models;
using System.Data;

namespace EventHub.Data
{ 
    public class EventHubDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<PostReaction> PostReactions { get; set; }

        public EventHubDbContext(DbContextOptions opts) : base(opts) { }

        protected override void OnModelCreating(ModelBuilder model)
        {
            model.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });
            model.Entity<PostReaction>()
                .HasKey(pr => pr.Id);
            // дальше конфигурация связей…
        }
    }
}
