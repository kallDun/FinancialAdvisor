using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Models.Telegram;
using Telegram.Bot.Types;

namespace FinancialAdvisorTelegramBot.Bot.Updates
{
    public interface ITelegramUpdateListener
    {        
        Task GetUpdate(UpdateArgs update, TelegramUser user);
    }
}
