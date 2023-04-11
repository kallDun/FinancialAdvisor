using FinancialAdvisorTelegramBot.Models.Telegram;
using Telegram.Bot.Types;

namespace FinancialAdvisorTelegramBot.Bot.Updates
{
    public class TelegramUpdateDistributor : ITelegramUpdateDistributor
    {
        private readonly ITelegramUpdateListenerContainer _listenerContainer;
        private readonly ITelegramAvailableListeners _availableListeners;
        private TelegramUser? _user;

        public TelegramUpdateDistributor(ITelegramUpdateListenerContainer listenerContainer, ITelegramAvailableListeners availableListeners)
        {
            _listenerContainer = listenerContainer;
            _availableListeners = availableListeners;
        }

        public async Task TryToSignIn(Update update)
        {
            if (update.Message is null) return;

            long chatId = update.Message.Chat.Id;

            List<ITelegramUpdateListener>? listeners = _listenerContainer.Listeners.GetValueOrDefault(chatId);
            if (listeners is null)
            {
                _listenerContainer.Listeners.Add(chatId, _availableListeners.Listeners);
            }

            // TODO: find or create telegram user
            _user = new();
        }

        public async Task GetUpdate(Update update)
        {
            if (update.Message is null || _user is null) return;

            long chatId = update.Message.Chat.Id;
            List<ITelegramUpdateListener>? listeners = _listenerContainer.Listeners.GetValueOrDefault(chatId);
            if (listeners is null) return;
            foreach (var listener in listeners)
            {
                await listener.GetUpdate(update, _user);
            }
        }
    }
}
