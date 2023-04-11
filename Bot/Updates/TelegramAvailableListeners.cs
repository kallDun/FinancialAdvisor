namespace FinancialAdvisorTelegramBot.Bot.Updates
{
    public class TelegramAvailableListeners : ITelegramAvailableListeners
    {
        public List<ITelegramUpdateListener> Listeners { get; }

        public TelegramAvailableListeners(params ITelegramUpdateListener[] listeners) 
        {
            Listeners = listeners.ToList();
        }
    }
}
