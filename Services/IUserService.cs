using FinancialAdvisorTelegramBot.Models;

namespace FinancialAdvisorTelegramBot.Services
{
    public interface IUserService
    {
        Task<int> AddUser(User user);

        Task<List<User>> GetUsers();

        Task<List<User>> GetUsersWithMoreOrEqualAccountsThan(int accountsCount);
    }
}
