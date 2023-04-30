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
    }
}
