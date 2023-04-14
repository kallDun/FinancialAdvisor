using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Bot.ReplyArgs;
using FinancialAdvisorTelegramBot.Models.Telegram;

namespace FinancialAdvisorTelegramBot.Bot.Views
{
    public class StartCommand : ICommand
    {
        public static string COMMAND_TEXT_STYLE => string.Empty;
        public static string COMMAND_DEFAULT_STYLE => "/start";

        private readonly IBot _bot;

        public StartCommand(IBot bot)
        {
            _bot = bot;
        }

        public bool CanExecute(UpdateArgs update, TelegramUser user) 
            => update.GetTextData() == COMMAND_DEFAULT_STYLE;

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            await _bot.Write(user, new TextMessageArgs 
            { 
                Text = $"Hi, {user.FirstName} {user.LastName}!" 
            });
        }
    }
}
