namespace FinancialAdvisorTelegramBot.Repositories
{
    public interface IRepository<T> : IReadonlyRepository<T>
    {
        Task Update(T entity);
        
        Task Delete(T entity);
    }
}
