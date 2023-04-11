using Telegram.Bot.Types;

namespace FinancialAdvisorTelegramBot.Bot.Updates
{
    public interface ITelegramUpdateDistributor
    {
        Task SignIn(Update update);

        Task GetUpdate(Update update);
    }
}
