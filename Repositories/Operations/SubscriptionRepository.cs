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

        public async Task<IList<Subscription>> GetAllWithData()
        {
            return await _context.Subscriptions
                .Include(x => x.Account)
                .Include(x => x.User).ThenInclude(x => x.TelegramUser)
                .Include(x => x.Category)
                .ToListAsync();
        }

        public async Task<Subscription?> GetByName(int userId, string name, bool loadAllData)
        {
            return loadAllData 
                ? await _context.Subscriptions.FirstOrDefaultAsync(s => s.UserId == userId && s.Name == name)
                : await _context.Subscriptions
                .Include(x => x.Account)
                .Include(x => x.User)
                .Include(x => x.Category)
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

        public async Task<IList<Subscription>> LoadAllWithDataByUser(int userId, string? accountName = null)
        {
            if (accountName is null)
            {
                return await _context.Subscriptions
                    .Where(s => s.UserId == userId)
                    .Include(x => x.User)
                    .Include(s => s.Account)
                    .Include(x => x.Category)
                    .ToListAsync();
            }
            else
            {
                return await _context.Subscriptions
                    .Include(s => s.Account)
                    .Include(s => s.Account)
                    .Include(x => x.Category)
                    .Where(s => s.UserId == userId && s.Account != null && s.Account.Name == accountName)
                    .ToListAsync();
            }
        }
    }
}
