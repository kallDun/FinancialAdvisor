using FinancialAdvisorTelegramBot.Data;
using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Utils.Attributes;
using Microsoft.EntityFrameworkCore;

namespace FinancialAdvisorTelegramBot.Repositories.Core
{
    [CustomRepository]
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _context;

        public CategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public DbContext DatabaseContext => _context;

        public DbSet<Category> DbSet => _context.Categories;

        public async Task<int> Count(int userId)
        {
            return await _context.Categories.CountAsync(c => c.UserId == userId);
        }

        public async Task<IList<Category>> GetCategoriesByUser(int userId)
        {
            return await DbSet.Where(x => x.UserId == userId).ToListAsync();
        }

        public async Task<Category?> GetCategoryByName(int userId, string name)
        {
            return await DbSet.FirstOrDefaultAsync(x => x.UserId == userId && x.Name == name);
        }

        public async Task<bool> HasAny(int userId)
        {
            return await DbSet.AnyAsync(x => x.UserId == userId);
        }

        public async Task<bool> IsCategoryNameUnique(int userId, string name)
        {
            return !(await DbSet.AnyAsync(x => x.UserId == userId && x.Name == name));
        }
    }
}
