namespace FinancialAdvisorTelegramBot.Bot.Updates
{
    public interface ITelegramAvailableListeners
    {
        List<ITelegramUpdateListener> Listeners { get; }
    }
}
