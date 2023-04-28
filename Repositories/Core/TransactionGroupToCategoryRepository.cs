using FinancialAdvisorTelegramBot.Data;
using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Utils.Attributes;
using Microsoft.EntityFrameworkCore;

namespace FinancialAdvisorTelegramBot.Repositories.Core
{
    [CustomRepository]
    public class TransactionGroupToCategoryRepository : ITransactionGroupToCategoryRepository
    {
        private readonly AppDbContext _context;

        public TransactionGroupToCategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public DbContext DatabaseContext => _context;

        public DbSet<TransactionGroupToCategory> DbSet => _context.TransactionGroupToCategories;

        public async Task<bool> Find(int transactionGroupId, int categoryId)
        {
            return await DbSet.AnyAsync(x => x.TransactionGroupId == transactionGroupId && x.CategoryId == categoryId);
        }
    }
}
