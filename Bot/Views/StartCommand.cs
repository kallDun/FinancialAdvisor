using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Telegram;
using Telegram.Bot.Types;

namespace FinancialAdvisorTelegramBot.Bot.Views
{
    public class StartCommand : ICommand
    {
        private IBot _bot;

        public StartCommand(IBot bot)
        {
            _bot = bot;
        }

        public bool CanExecute(Update update, TelegramUser user) => update.Message?.Text == "/start";

        public async Task Execute(Update update, TelegramUser user)
        {
            await _bot.Write($"Hi, {user.FirstName} {user.LastName}!", user.ChatId);
        }
    }
}
