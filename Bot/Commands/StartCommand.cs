using FinancialAdvisorTelegramBot.Models.Telegram;
using Telegram.Bot.Types;

namespace FinancialAdvisorTelegramBot.Bot.Commands
{
    public class StartCommand : ICommand
    {
        public string Name => "/hi";

        public bool IsFinished { get; private set; } = false;

        private IBot _bot;
        
        public StartCommand(IBot bot)
        {
            _bot = bot;
        }

        public async Task Execute(Update update, TelegramUser user)
        {
            await _bot.Write($"Hi, {update.Message.Chat.Username}!", update.Message.Chat.Id);
        }
    }
}
