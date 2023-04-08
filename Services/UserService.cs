using FinancialAdvisorTelegramBot.Data;
using FinancialAdvisorTelegramBot.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FinancialAdvisorTelegramBot.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext dbContext)
        {
            _context = dbContext;
        }
        
        public async Task<int> AddUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user.Id;
        }

        public async Task<List<User>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<List<User>> GetUsersWithMoreOrEqualAccountsThan(int accountsCount)
        {
            string sql = "SELECT * FROM users WHERE id IN " +
                "(SELECT user_id FROM accounts GROUP BY user_id HAVING COUNT(*) >= @Count)";
            NpgsqlParameter[] parameters = new NpgsqlParameter[]
            {
                new NpgsqlParameter("@Count", accountsCount)
            };
            return await _context.Users.FromSqlRaw(sql, parameters).ToListAsync();
        }
    }
}
