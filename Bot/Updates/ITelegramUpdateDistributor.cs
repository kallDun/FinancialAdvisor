using Telegram.Bot.Types;

namespace FinancialAdvisorTelegramBot.Bot.Updates
{
    public interface ITelegramUpdateDistributor
    {
        Task TryToSignIn(Update update);

        Task GetUpdate(Update update);
    }
}
