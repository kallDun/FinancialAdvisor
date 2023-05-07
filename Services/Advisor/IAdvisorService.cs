using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Telegram;

namespace FinancialAdvisorTelegramBot.Services.Advisor
{
    public interface IAdvisorService
    {
        void WriteSimpleAdvice(TelegramUser user, User profile);

        void WriteAdvancedAdviceUsingMonthlyStatistics(TelegramUser user, User profile);
        
        void WriteAdvancedAdviceUsingWeeklyStatistics(TelegramUser user, User profile);
    }
}
