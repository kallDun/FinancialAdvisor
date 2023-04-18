using FinancialAdvisorTelegramBot.Data;
using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Utils.Attributes;
using Microsoft.EntityFrameworkCore;

namespace FinancialAdvisorTelegramBot.Repositories.Core
{
    [CustomRepository]
    public class TransactionRepository : ITransactionRepository
    {
        private readonly AppDbContext _context;

        public TransactionRepository(AppDbContext context)
        {
            _context = context;
        }

        public DbContext DatabaseContext => _context;

        public DbSet<Transaction> DbSet => _context.Transactions;

        public async Task<bool> HasAnyTransactionByCategory(int categoryId)
        {
            return await _context.Transactions.AnyAsync(x => x.CategoryId == categoryId);
        }
    }
}
