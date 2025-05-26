using ChatService.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace ChatService.DB
{
    public class Context : DbContext
    {
        public Context(DbContextOptions options) : base(options)
        { }

        public DbSet<ChatMessage> Messages { get; set; }
        public DbSet<ChatGroup> Groups { get; set; }
        public DbSet<ChatGroupMember> GroupMembers { get; set; }
        public DbSet<UserConnection> UserConnections { get; set; }
        public DbSet<UnreadMessageCounter> UnreadMessageCounters { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());


        }
    }
}
