using Microsoft.EntityFrameworkCore;

namespace FinancialAdvisorTelegramBot.Repositories
{
    public interface IReadonlyRepository<T> where T : class
    {
        DbContext DatabaseContext { get; }

        DbSet<T> DbSet { get; }

        async Task<IList<T>> GetAll()
        {
            return await DbSet.ToListAsync();
        }
        
        async Task<T?> GetById(int id)
        {
            return await DbSet.FindAsync(id);
        }

        async Task<T> Add(T entity)
        {
            await DbSet.AddAsync(entity);
            await DatabaseContext.SaveChangesAsync();
            return entity;
        }
    }
}
