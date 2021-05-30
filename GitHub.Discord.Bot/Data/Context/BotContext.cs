using GitHub.Discord.Bot.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace GitHub.Discord.Bot.Data.Context
{
    public class BotContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<OptIn> OptIns { get; set; }
        public DbSet<IssueReference> IssueReferences { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite(Bot.BaseConfiguration.DbConnectionString);
    }
}