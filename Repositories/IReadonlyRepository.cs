namespace FinancialAdvisorTelegramBot.Repositories
{
    public interface IReadonlyRepository<T>
    {
        Task<IList<T>> GetAll();   
        
        Task<T?> Get(int id);

        Task<int> Add(T entity);
    }
}
