using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Telegram;

namespace FinancialAdvisorTelegramBot.Services.Advisor
{
    public interface IAdvisorService
    {
        void WriteSimpleAdviceInBackground(TelegramUser user, User profile);
    }
}
