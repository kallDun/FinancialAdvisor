using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Bot.ReplyArgs;
using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;

namespace FinancialAdvisorTelegramBot.Bot.Views.Profile
{
    public class OpenProfileMenuCommand : ICommand
    {
        public static string COMMAND_TEXT_STYLE => "Profile menu";
        public static string COMMAND_DEFAULT_STYLE => "/profile_menu";

        private readonly IBot _bot;
        private readonly IUserService _userService;

        public OpenProfileMenuCommand(IBot bot, IUserService userService)
        {
            _bot = bot;
            _userService = userService;
        }

        public bool CanExecute(UpdateArgs update, TelegramUser user) 
            => update.GetTextData() == COMMAND_DEFAULT_STYLE 
            || update.GetTextData() == COMMAND_TEXT_STYLE;

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            User? profile = user.UserId != null 
                ? await _userService.GetById((int)user.UserId) : null;

            IEnumerable<string> buttons = profile == null
                ? new List<string>()
                {
                    StartCommand.COMMAND_DEFAULT_STYLE,
                    HelpCommand.COMMAND_DEFAULT_STYLE
                }
                : new List<string>()
                {
                    HelpCommand.COMMAND_DEFAULT_STYLE,
                    StartCommand.COMMAND_DEFAULT_STYLE
                };

            await _bot.SendInlineKeyboard(user, new InlineKeyboardArgs
            {
                Text = "<b>↓ profile ↓ menu ↓</b>",
                Buttons = buttons,
                Placeholder = "Type profile command"
            });
        }
    }
}
