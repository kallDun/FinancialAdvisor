using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Telegram;
using Telegram.Bot.Types;

namespace FinancialAdvisorTelegramBot.Bot.Views
{
    public class HelpCommand : ICommand
    {
        private IBot _bot;

        public HelpCommand(IBot bot)
        {
            _bot = bot;
        }

        public bool CanExecute(Update update, TelegramUser user) => update.Message?.Text == "/help";

        public async Task Execute(Update update, TelegramUser user)
        {
            await _bot.Write($"Help command", user.ChatId);
        }
    }
}
