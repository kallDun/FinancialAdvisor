namespace FinancialAdvisorTelegramBot.Bot.Updates
{
    public interface ITelegramUpdateListenerContainer
    {
        Dictionary<long, List<ITelegramUpdateListener>> Listeners { get; }
    }
}
