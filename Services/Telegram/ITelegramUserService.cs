using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Telegram;

namespace FinancialAdvisorTelegramBot.Services.Telegram
{
    public interface ITelegramUserService
    {
        Task<TelegramUser> GetExistingOrCreateNewTelegramUser(long chatId, long telegramId,
            string? username, string? firstName, string? lastName);

        Task SaveCurrentCommand(TelegramUser user, ICommand? command);
        
        ICommand? GetCurrentCommand(TelegramUser user);

        /*Task DeleteProfile(TelegramUser user);*/
    }
}
