using System;
using Microsoft.EntityFrameworkCore;
using static SpeedDatingBot.Helpers;

namespace SpeedDatingBot.Models
{
    public class DiscordContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(Env("DBConnectionString"));
        }
    }
}