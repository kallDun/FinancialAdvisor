using FinancialAdvisorTelegramBot.Models.Telegram;
using Telegram.Bot.Types;

namespace FinancialAdvisorTelegramBot.Bot.Updates
{
    public interface ITelegramUpdateListener
    {        
        Task GetUpdate(Update update, TelegramUser user);
    }
}
