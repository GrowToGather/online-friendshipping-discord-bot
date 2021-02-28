using Microsoft.EntityFrameworkCore;

namespace SpeedDatingBot.Models
{
    public class DiscordContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(
                "Host=localhost;Database=online-friendshipping;Username=nathaniel;Password=friendship");
        }
    }
}