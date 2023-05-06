using FinancialAdvisorTelegramBot.Data;
using FinancialAdvisorTelegramBot.Models.Operations;
using FinancialAdvisorTelegramBot.Utils.Attributes;
using Microsoft.EntityFrameworkCore;

namespace FinancialAdvisorTelegramBot.Repositories.Operations
{
    [CustomRepository]
    public class TargetRepository : ITargetRepository
    {
        private readonly AppDbContext _context;

        public TargetRepository(AppDbContext context)
        {
            _context = context;
        }

        public DbContext DatabaseContext => _context;

        public DbSet<TargetSubAccount> DbSet => _context.TargetSubAccounts;

        public async Task<int> Count(int accountId)
        {
            return await DbSet.Where(x => x.AccountId == accountId).CountAsync();
        }

        public async Task<IList<TargetSubAccount>> GetAll(int userId, string accountName)
        {
            return await DbSet
                .Include(x => x.Account)
                .Where(x => x.Account.UserId == userId && x.Account.Name == accountName)
                .ToListAsync();
        }

        public async Task<TargetSubAccount?> GetByName(int userId, string accountName, string targetName)
        {
            return await DbSet
                .Include(x => x.Account)
                .FirstOrDefaultAsync(x => x.Account.UserId == userId && x.Account.Name == accountName && x.Name == targetName);
        }

        public async Task<TargetSubAccount?> GetByName(int accountId, string targetName)
        {
            return await DbSet.FirstOrDefaultAsync(x => x.AccountId == accountId && x.Name == targetName);
        }

        public async Task<bool> HasAny(int userId, string accountName)
        {
            return await DbSet
                .Include(x => x.Account)
                .AnyAsync(x => x.Account.UserId == userId && x.Account.Name == accountName);
        }

        public async Task<bool> IsUnique(int accountId, string name)
        {
            return await DbSet.AnyAsync(x => x.AccountId == accountId && x.Name == name) == false;
        }
    }
}
