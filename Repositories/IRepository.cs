namespace FinancialAdvisorTelegramBot.Repositories
{
    public interface IRepository<T> : IReadonlyRepository<T>
    {
        Task<T> Update(T entity);
        
        Task Delete(T entity);
    }
}
