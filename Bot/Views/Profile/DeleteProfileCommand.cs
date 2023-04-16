using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;
using FinancialAdvisorTelegramBot.Services.Telegram;
using FinancialAdvisorTelegramBot.Utils.CommandSerializing;

namespace FinancialAdvisorTelegramBot.Bot.Views.Profile
{
    public class DeleteProfileCommand : ICommand
    {
        public static string TEXT_STYLE => "Delete profile";
        public static string DEFAULT_STYLE => "/profile_delete";
        public virtual bool IsFinished { get; private set; } = false;

        [CommandPropertySerializable] public int Status { get; set; }

        private readonly IBot _bot;
        private readonly IUserService _userService;
        private readonly ITelegramUserService _telegramUserService;

        public DeleteProfileCommand(IBot bot, IUserService userService, ITelegramUserService telegramUserService)
        {
            _bot = bot;
            _userService = userService;
            _telegramUserService = telegramUserService;
        }

        public bool CanExecute(UpdateArgs update, TelegramUser user)
            => (update.GetTextData() == DEFAULT_STYLE
            || update.GetTextData() == TEXT_STYLE)
            && user.UserId is not null;

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            const string CONFIRM_COMMAND = "/profile_delete_confirm";
            
            if (Status == 0)
            {
                await _bot.Write(user, new TextMessageArgs
                {
                    Text = "Are you sure you want to delete your profile with ALL data?",
                    MarkupType = ReplyMarkupType.InlineKeyboard,
                    InlineKeyboardButtons = new()
                    {
                        new()
                        {
                            new InlineButton("Yes", CONFIRM_COMMAND)
                        },
                        new()
                        {
                            new InlineButton("No", GeneralCommands.Cancel)
                        }
                    }
                });
                Status++;
            }
            else
            {
                if (update.GetTextData() == CONFIRM_COMMAND)
                {
                    if (user.UserId is null) throw new InvalidOperationException("User id is null");
                    /*await _telegramUserService.DeleteProfile(user);*/

                    await _bot.Write(user, new TextMessageArgs
                    {
                        Text = "Your profile has been deleted"
                    });
                }
            }
        }


    }
}
