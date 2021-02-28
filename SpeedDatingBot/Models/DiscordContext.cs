using System;
using Microsoft.EntityFrameworkCore;
using static SpeedDatingBot.Helpers;

namespace SpeedDatingBot.Models
{
    public class DiscordContext : DbContext
    {
        private Config _systemConfig;

        public DiscordContext()
        {
            _systemConfig = new Config();
        }
        
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_systemConfig.ConfigData.DbConnectionString);
        }
    }
}