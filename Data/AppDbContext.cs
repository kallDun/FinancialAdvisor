using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Operations;
using FinancialAdvisorTelegramBot.Models.Telegram;
using Microsoft.EntityFrameworkCore;

namespace FinancialAdvisorTelegramBot.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<TelegramUser> TelegramUsers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<LimitByCategory> CategoryLimits { get; set; }
        public DbSet<TransactionGroup> TransactionGroups { get; set; }
        public DbSet<TransactionGroupToCategory> TransactionGroupToCategories { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        {
            Database.EnsureDeleted();
            Database.EnsureCreated();
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
