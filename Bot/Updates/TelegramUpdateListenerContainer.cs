namespace FinancialAdvisorTelegramBot.Bot.Updates
{
    public class TelegramUpdateListenerContainer : ITelegramUpdateListenerContainer
    {
        public Dictionary<long, List<ITelegramUpdateListener>> Listeners { get; }

        public TelegramUpdateListenerContainer()
        {
            Listeners = new Dictionary<long, List<ITelegramUpdateListener>>();
        }
    }
}
