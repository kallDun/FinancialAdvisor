using FinancialAdvisorTelegramBot.Models.Core;

namespace FinancialAdvisorTelegramBot.Repositories.Core
{
    public interface IUserRepository : IRepository<User>
    {
        Task DeleteById(int userId);
    }
}
