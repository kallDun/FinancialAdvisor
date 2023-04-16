using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Bot.Views.Profile;
using FinancialAdvisorTelegramBot.Models.Telegram;

namespace FinancialAdvisorTelegramBot.Bot.Views
{
    public class HelpCommand : ICommand
    {
        public static string TEXT_STYLE => "Help";
        public static string DEFAULT_STYLE => GeneralCommands.Help;

        private readonly IBot _bot;

        public HelpCommand(IBot bot)
        {
            _bot = bot;
        }

        public bool CanExecute(UpdateArgs update, TelegramUser user) 
            => update.GetTextData() == DEFAULT_STYLE
            || update.GetTextData() == TEXT_STYLE;

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            await ExecuteStatic(_bot, user);
        }
        
        public static async Task ExecuteStatic(IBot bot, TelegramUser user)
        {
            List<string> buttons = user.UserId != null
                ? new List<string>()
                {
                    OpenProfileMenuCommand.TEXT_STYLE
                }
                : new List<string>()
                {
                    OpenProfileMenuCommand.TEXT_STYLE
                };

            await bot.Write(user, new TextMessageArgs
            {
                Text = "<b>↓ Available commands ↓</b>",
                Placeholder = "Type command",
                MarkupType = ReplyMarkupType.ReplyKeyboard,
                ReplyKeyboardButtons = buttons,
            });
        }
    }
}
