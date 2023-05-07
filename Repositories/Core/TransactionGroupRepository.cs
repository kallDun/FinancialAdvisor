using FinancialAdvisorTelegramBot.Data;
using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Utils.Attributes;
using Microsoft.EntityFrameworkCore;

namespace FinancialAdvisorTelegramBot.Repositories.Core
{
    [CustomRepository]
    public class TransactionGroupRepository : ITransactionGroupRepository
    {
        private readonly AppDbContext _context;

        public TransactionGroupRepository(AppDbContext context)
        {
            _context = context;
        }

        public DbContext DatabaseContext => _context;

        public DbSet<TransactionGroup> DbSet => _context.TransactionGroups;

        public async Task<TransactionGroup?> GetByIndex(int accountId, int index)
        {
            return await _context.TransactionGroups
                .FirstOrDefaultAsync(group => group.AccountId == accountId && group.Index == index);
        }

        public async Task<IList<TransactionGroup>> GetGroupsBetweenIndexesByUser(int userId, int indexFrom, int indexTo)
        {
            return await _context.TransactionGroups
                .Include(x => x.Account)
                .Include(x => x.TransactionGroupToCategories).ThenInclude(x => x.Category)
                .Where(group => group.Account.UserId == userId && group.Index >= indexFrom && group.Index <= indexTo)
                .ToListAsync();
        }
    }
}
