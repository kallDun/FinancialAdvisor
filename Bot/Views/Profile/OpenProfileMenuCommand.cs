using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;

namespace FinancialAdvisorTelegramBot.Bot.Views.Profile
{
    public class OpenProfileMenuCommand : ICommand
    {
        public static string TEXT_STYLE => "Profile menu";
        public static string DEFAULT_STYLE => "/profile_menu";

        private readonly IBot _bot;
        private readonly IUserService _userService;

        public OpenProfileMenuCommand(IBot bot, IUserService userService)
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
                    CreateProfileCommand.TEXT_STYLE,
                    HelpCommand.TEXT_STYLE
                }
                : new List<string>()
                {
                    UpdateProfileCommand.TEXT_STYLE,
                    DeleteProfileCommand.TEXT_STYLE,
                    HelpCommand.TEXT_STYLE
                };

            await _bot.Write(user, new TextMessageArgs
            {
                Text = "<b>↓ profile ↓ menu ↓</b>",
                Placeholder = "Type profile command",
                MarkupType = ReplyMarkupType.ReplyKeyboard,
                ReplyKeyboardButtons = buttons,
            });
        }
    }
}
