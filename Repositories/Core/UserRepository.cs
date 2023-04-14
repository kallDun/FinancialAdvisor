using FinancialAdvisorTelegramBot.Data;
using FinancialAdvisorTelegramBot.Models.Core;
using Microsoft.EntityFrameworkCore;

namespace FinancialAdvisorTelegramBot.Repositories.Core
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext dbContext)
        {
            _context = dbContext;
        }

        public async Task<int> Add(User entity)
        {
            entity.CreatedAt = DateTime.Now;
            _context.Users.Add(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        public async Task Delete(User entity)
        {
            _context.Users.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<User?> GetById(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<IList<User>> GetAll()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User> Update(User entity)
        {
            entity.UpdatedAt = DateTime.Now;
            _context.Users.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
    }
}
