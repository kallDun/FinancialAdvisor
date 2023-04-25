using FinancialAdvisorTelegramBot.Data;
using FinancialAdvisorTelegramBot.Models.Operations;
using FinancialAdvisorTelegramBot.Utils.Attributes;
using Microsoft.EntityFrameworkCore;

namespace FinancialAdvisorTelegramBot.Repositories.Operations
{
    [CustomRepository]
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly AppDbContext _context;

        public SubscriptionRepository(AppDbContext context)
        {
            _context = context;
        }

        public DbContext DatabaseContext => _context;

        public DbSet<Subscription> DbSet => _context.Subscriptions;

        
        public async Task<Subscription?> GetByName(int userId, string name)
        {
            return await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Name == name);
        }

        public async Task<bool> HasAny(int userId, string? accountName = null)
        {
            if (accountName is null)
            {
                return await _context.Subscriptions.AnyAsync(s => s.UserId == userId);
            }
            else
            {
                return await _context.Subscriptions
                    .Include(s => s.Account)
                    .AnyAsync(s => s.UserId == userId && s.Account != null && s.Account.Name == accountName);
            }
        }

        public async Task<IList<Subscription>> LoadAllWithAccounts(int userId, string? accountName = null)
        {
            if (accountName is null)
            {
                return await _context.Subscriptions
                    .Where(s => s.UserId == userId)
                    .Include(s => s.Account)
                    .ToListAsync();
            }
            else
            {
                return await _context.Subscriptions
                    .Include(s => s.Account)
                    .Where(s => s.UserId == userId && s.Account != null && s.Account.Name == accountName)
                    .ToListAsync();
            }
        }
    }
}
