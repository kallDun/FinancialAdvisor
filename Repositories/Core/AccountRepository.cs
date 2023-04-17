using FinancialAdvisorTelegramBot.Data;
using FinancialAdvisorTelegramBot.Models.Core;
using Microsoft.EntityFrameworkCore;

namespace FinancialAdvisorTelegramBot.Repositories.Core
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AppDbContext _context;

        public AccountRepository(AppDbContext context)
        {
            _context = context;
        }


        public async Task<int> Add(Account entity)
        {
            entity.CreatedAt = DateTime.Now;
            _context.Accounts.Add(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        public async Task Delete(Account entity)
        {
            _context.Accounts.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IList<Account>> GetAll()
        {
            return await _context.Accounts.ToListAsync();
        }

        public async Task<Account?> GetById(int id)
        {
            return await _context.Accounts.FindAsync(id);
        }

        public async Task<Account> Update(Account entity)
        {
            entity.UpdatedAt = DateTime.Now;
            _context.Accounts.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<IList<Account>> GetAccountsByUserId(int userId)
        {
            return await _context.Accounts
                .Where(x => x.UserId == userId)
                .ToListAsync();
        }
    }
}
