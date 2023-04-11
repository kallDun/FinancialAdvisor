using FinancialAdvisorTelegramBot.Models.Telegram;

namespace FinancialAdvisorTelegramBot.Repositories.Telegram
{
    public interface ITelegramUserRepository : IRepository<TelegramUser>
    {
        Task<TelegramUser?> GetByChatId(long chatId);
    }
}
