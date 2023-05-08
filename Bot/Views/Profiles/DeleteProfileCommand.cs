using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;
using FinancialAdvisorTelegramBot.Utils.CommandSerializing;

namespace FinancialAdvisorTelegramBot.Bot.Views.Profiles
{
    public class DeleteProfileCommand : ICommand
    {
        public static string TEXT_STYLE => "Delete profile";
        public static string DEFAULT_STYLE => "/delete";
        public virtual bool IsFinished { get; private set; } = false;
        public bool ShowContextMenuAfterExecution => true;

        [CommandPropertySerializable] public int Status { get; set; }

        private readonly IBot _bot;
        private readonly IUserService _userService;

        public DeleteProfileCommand(IBot bot, IUserService userService)
        {
            _bot = bot;
            _userService = userService;
        }

        public bool CanExecute(UpdateArgs update, TelegramUser user)
            => user.ContextMenu == ContextMenus.Profile
            && (update.GetTextData() == DEFAULT_STYLE || update.GetTextData() == TEXT_STYLE)
            && user.UserId is not null;

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            string text = update.GetTextData();

            if (Status == 0)
            {
                await AskForDelete(user);
                Status++;
            }
            else
            {
                await ProcessDeletion(user, text);
                IsFinished = true;
            }
        }

        private async Task ProcessDeletion(TelegramUser user, string text)
        {
            if (text == GeneralCommands.Confirm)
            {
                if (user.UserId is null) throw new InvalidOperationException("User id is null");
                await _userService.DeleteById((int)user.UserId);

                await _bot.Write(user, new TextMessageArgs
                {
                    Text = "Your profile has been deleted"
                });
            }
        }

        private async Task AskForDelete(TelegramUser user)
        {
            await _bot.Write(user, new TextMessageArgs
            {
                Text = "Are you sure you want to delete your profile with ALL data?",
                MarkupType = ReplyMarkupType.InlineKeyboard,
                InlineKeyboardButtons = new()
                    {
                        new()
                        {
                            new InlineButton("Yes", GeneralCommands.Confirm)
                        },
                        new()
                        {
                            new InlineButton("No", GeneralCommands.Cancel)
                        }
                    }
            });
        }
    }
}