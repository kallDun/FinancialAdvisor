using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;

namespace FinancialAdvisorTelegramBot.Bot.Views.Profiles
{
    public class WatchProfileCommand : ICommand
    {
        public static string TEXT_STYLE => "View profile";
        public static string DEFAULT_STYLE => "/view";

        private readonly IBot _bot;
        private readonly IUserService _userService;

        public WatchProfileCommand(IBot bot, IUserService userService)
        {
            _bot = bot;
            _userService = userService;
        }

        public bool CanExecute(UpdateArgs update, TelegramUser user)
            => user.ContextMenu == ContextMenus.Profile
            && (update.GetTextData() == DEFAULT_STYLE || update.GetTextData() == TEXT_STYLE)
            && user.UserId != null;

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            User? profile = user.UserId != null
                ? await _userService.GetById((int)user.UserId) : null;
            if (profile is null) throw new InvalidDataException("Profile not found");
            
            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"<u><b>Profile:</b></u>" +
                $"\nName: <code>{profile.FirstName}</code>" +
                $"\nSurname: <code>{(string.IsNullOrEmpty(profile.LastName) ? "none" : profile.LastName)}</code>" +
                $"\nEmail: <code>{(string.IsNullOrEmpty(profile.Email) ? "none" : profile.Email)}</code>"
            });
        }
    }
}
