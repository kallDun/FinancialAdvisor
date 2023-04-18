﻿using FinancialAdvisorTelegramBot.Data;
using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Utils.Attributes;
using Microsoft.EntityFrameworkCore;

namespace FinancialAdvisorTelegramBot.Repositories.Core
{
    [CustomRepository]
    public class AccountRepository : IAccountRepository
    {
        private readonly AppDbContext _context;
        
        public AccountRepository(AppDbContext context)
        {
            _context = context;
        }

        public DbContext DatabaseContext => _context;

        public DbSet<Account> DbSet => _context.Accounts;

        public Task<Account?> GetAccountByName(int userId, string name)
        {
            return _context.Accounts.FirstOrDefaultAsync(a => a.UserId == userId && a.Name == name);
        }

        public async Task<IList<Account>> GetAccountsByUserId(int userId)
        {
            return await DbSet
                .Where(x => x.UserId == userId)
                .ToListAsync();
        }

        public async Task<bool> IsAccountNameUnique(int userId, string name)
        {
            return !(await DbSet.AnyAsync(x => x.UserId == userId && x.Name == name));
        }
    }
}
