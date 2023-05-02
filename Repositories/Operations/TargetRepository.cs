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

        public async Task<bool> HasAny(int userId, string accountName)
        {
            return await _context.TargetSubAccounts
                .Include(x => x.Account)
                .AnyAsync(x => x.Account.UserId == userId 
                            && x.Account.Name == accountName);
        }
    }
}
