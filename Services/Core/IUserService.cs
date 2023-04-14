using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Telegram;

namespace FinancialAdvisorTelegramBot.Services.Core
{
    public interface IUserService
    {
        Task<User?> GetById(int userId);

        Task<User> Create(TelegramUser telegramUser, string first_name, string? last_name, string? email);

        Task<User> Update(User user, string first_name, string? last_name, string? email);

        Task Delete(User user);
    }
}
