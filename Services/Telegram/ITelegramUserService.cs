using FinancialAdvisorTelegramBot.Models.Telegram;

namespace FinancialAdvisorTelegramBot.Services.Telegram
{
    public interface ITelegramUserService
    {
        Task<TelegramUser> GetExistingOrCreateNewTelegramUser(long chatId, string? username, string? firstName, string? lastName);
    }
}
