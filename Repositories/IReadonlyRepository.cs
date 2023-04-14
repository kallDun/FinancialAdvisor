namespace FinancialAdvisorTelegramBot.Repositories
{
    public interface IReadonlyRepository<T>
    {
        Task<IList<T>> GetAll(); 
        
        Task<T?> GetById(int id);

        Task<int> Add(T entity);
    }
}
