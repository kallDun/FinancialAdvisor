using FinancialAdvisorTelegramBot.Bot.Args;

namespace FinancialAdvisorTelegramBot.Bot.Updates
{
    public interface ITelegramUpdateDistributor
    {
        Task SignIn(UpdateArgs update);

        Task GetUpdate(UpdateArgs update);
    }
}
