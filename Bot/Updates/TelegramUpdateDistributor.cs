using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Telegram;

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

        public async Task SignIn(UpdateArgs update)
        {
            if (update.From is null) return;
            _user = await _telegramUserService.GetExistingOrCreateNewTelegramUser(
                update.ChatId, update.From.Id, update.From.Username, 
                update.From.FirstName, update.From.LastName);
        }

        public async Task GetUpdate(UpdateArgs update)
        {
            if (_user is null) return;

            foreach (var listener in _availableListeners.Listeners)
            {
                await listener.GetUpdate(update, _user);
            }
        }
    }
}
