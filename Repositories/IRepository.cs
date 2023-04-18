namespace FinancialAdvisorTelegramBot.Repositories
{
    public interface IRepository<T> : IReadonlyRepository<T> where T : class
    {
        async Task<T> Update(T entity)
        {
            DbSet.Update(entity);
            await DatabaseContext.SaveChangesAsync();
            return entity;
        }
        
        async Task Delete(T entity)
        {
            DbSet.Remove(entity);
            await DatabaseContext.SaveChangesAsync();
        }
    }
}
