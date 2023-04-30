using FinancialAdvisorTelegramBot.Data;
using FinancialAdvisorTelegramBot.Models.Operations;
using FinancialAdvisorTelegramBot.Utils.Attributes;
using Microsoft.EntityFrameworkCore;

namespace FinancialAdvisorTelegramBot.Repositories.Operations
{
    [CustomRepository]
    public class LimitByCategoryRepository : ILimitByCategoryRepository
    {
        private readonly AppDbContext _context;

        public LimitByCategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public DbContext DatabaseContext => _context;

        public DbSet<LimitByCategory> DbSet => _context.CategoryLimits;

        public async Task<bool> IsLimitExpenseUnique(int categoryId, decimal limit)
        {
            return !(await DbSet.AnyAsync(x => x.CategoryId == categoryId && x.ExpenseLimit == limit));
        }

        public async Task<IList<LimitByCategory>> GetByCategoryWithInfo(int userId, string categoryName)
        {
            return await DbSet
                .Include(x => x.Account)
                .Include(x => x.Category)
                .Where(x => x.UserId == userId && x.Category.Name == categoryName)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalExpenseAmount(LimitByCategory limitByCategory, int currentGroupIndex)
        {
            int indexStart = GetIndexStartForLimit(limitByCategory, currentGroupIndex);
            int indexEnd = indexStart + limitByCategory.GroupCount;

            return await _context.TransactionGroupToCategories
                    .Include(x => x.TransactionGroup).ThenInclude(group => group.Account).ThenInclude(account => account.User)
                    .Where(x => x.CategoryId == limitByCategory.CategoryId
                        && (limitByCategory.AccountId == null
                            ? x.TransactionGroup.Account.UserId == limitByCategory.UserId
                            : x.TransactionGroup.AccountId == limitByCategory.AccountId)
                        && x.TransactionGroup.Index >= indexStart && x.TransactionGroup.Index < indexEnd)
                    .SumAsync(x => x.TotalExpense);
        }

        private int GetIndexStartForLimit(LimitByCategory limitByCategory, int currentGroupIndex)
        {
            var index = currentGroupIndex - limitByCategory.GroupIndexFrom; // 4
            return (index / limitByCategory.GroupCount) * limitByCategory.GroupCount + limitByCategory.GroupIndexFrom;
        }

        public async Task<bool> HasAny(int userId, string categoryName)
        {
            return await DbSet
                .Include(x => x.Category)
                .Where(x => x.UserId == userId && x.Category.Name == categoryName)
                .AnyAsync();
        }
    }
}
