using FinancialAdvisorTelegramBot.Models.Telegram;
using Microsoft.EntityFrameworkCore;

namespace FinancialAdvisorTelegramBot.Data
{
    public class TelegramDbContext : DbContext
    {
        public DbSet<TelegramUser> TelegramUsers { get; set; }

        public TelegramDbContext(DbContextOptions<TelegramDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(TelegramDbContext).Assembly);
        }
    }
}
