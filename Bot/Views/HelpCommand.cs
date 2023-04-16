using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Bot.Views.Profile;
using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;

namespace FinancialAdvisorTelegramBot.Bot.Views
{
    public class HelpCommand : ICommand
    {
        public static string TEXT_STYLE => "Back to main menu";
        public static string DEFAULT_STYLE => GeneralCommands.Help;

        private readonly IBot _bot;
        private readonly IUserService _userService;

        public HelpCommand(IBot bot, IUserService userService)
        {
            _bot = bot;
            _userService = userService;
        }

        public bool CanExecute(UpdateArgs update, TelegramUser user) 
            => update.GetTextData() == DEFAULT_STYLE
            || update.GetTextData() == TEXT_STYLE;

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            User? profile = user.UserId != null
                ? await _userService.GetById((int)user.UserId) : null;

            List<string> buttons = profile == null
                ? new List<string>()
                {
                    OpenProfileMenuCommand.TEXT_STYLE
                }
                : new List<string>()
                {
                    OpenProfileMenuCommand.TEXT_STYLE
                };

            await _bot.Write(user, new TextMessageArgs
            {
                Text = "<b>↓ Available commands ↓</b>",
                Placeholder = "Type command",
                MarkupType = ReplyMarkupType.ReplyKeyboard,
                ReplyKeyboardButtons = buttons,
            });
        }
    }
}
