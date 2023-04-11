using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Telegram;
using Telegram.Bot.Types;

namespace FinancialAdvisorTelegramBot.Bot.Updates
{
    public class TelegramUpdateDistributor : ITelegramUpdateDistributor
    {
        private readonly ITelegramAvailableListeners _availableListeners;
        private readonly ITelegramUserService _telegramUserService;
        private TelegramUser? _user;

        public TelegramUpdateDistributor(ITelegramAvailableListeners availableListeners, ITelegramUserService telegramUserService)
        {
            _availableListeners = availableListeners;
            _telegramUserService = telegramUserService;
        }

        public async Task SignIn(Update update)
        {
            if (update.Message is null) return;
            var chat = update.Message.Chat;
            _user = await _telegramUserService.GetExistingOrCreateNewTelegramUser(chat.Id, chat.Username, chat.FirstName, chat.LastName);
        }

        public async Task GetUpdate(Update update)
        {
            if (update.Message is null || _user is null) return;

            foreach (var listener in _availableListeners.Listeners)
            {
                await listener.GetUpdate(update, _user);
            }
        }
    }
}
