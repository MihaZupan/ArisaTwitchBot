using ArisaTwitchBot.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace ArisaTwitchBot.Database
{
    public class UsersDbContext : DbContext
    {
        public DbSet<UserModel> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=bot.db");
        }
    }
}
