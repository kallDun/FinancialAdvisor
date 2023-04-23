using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;
using FinancialAdvisorTelegramBot.Services.Telegram;

namespace FinancialAdvisorTelegramBot.Bot.Views.Profiles
{
    public class ProfileMenuCommand : ICommand
    {
        public static string TEXT_STYLE => "Profile menu";
        public static string DEFAULT_STYLE => "/profile_menu";

        private readonly IBot _bot;
        private readonly IUserService _userService;
        private readonly ITelegramUserService _telegramUserService;

        public ProfileMenuCommand(IBot bot, IUserService userService, ITelegramUserService telegramUserService)
        {
            _bot = bot;
            _userService = userService;
            _telegramUserService = telegramUserService;
        }

        public bool IsContextMenu(string contextMenu) => contextMenu == ContextMenus.Profile;

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
                    MainMenuCommand.TEXT_STYLE
                }
                : new List<string>()
                {
                    ViewProfileCommand.TEXT_STYLE,
                    UpdateProfileCommand.TEXT_STYLE,
                    DeleteProfileCommand.TEXT_STYLE,
                    MainMenuCommand.TEXT_STYLE
                };

            await _telegramUserService.SetContextMenu(user, ContextMenus.Profile);

            await _bot.Write(user, new TextMessageArgs
            {
                Text = "<b>↓ Profile menu ↓</b>",
                Placeholder = "Select command",
                MarkupType = ReplyMarkupType.ReplyKeyboard,
                ReplyKeyboardButtons = buttons,
            });
        }
    }
}
