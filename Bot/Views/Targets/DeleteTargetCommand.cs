using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Operations;
using FinancialAdvisorTelegramBot.Services.Telegram;
using FinancialAdvisorTelegramBot.Utils.CommandSerializing;

namespace FinancialAdvisorTelegramBot.Bot.Views.Targets
{
    public class DeleteTargetCommand : ICommand
    {
        public static string TEXT_STYLE => "Delete target";
        public static string DEFAULT_STYLE => "/delete";
        public virtual bool IsFinished { get; private set; } = false;
        public bool ShowContextMenuAfterExecution => true;

        [CommandPropertySerializable] public int Status { get; set; }

        private readonly IBot _bot;
        private readonly ITelegramUserService _telegramUserService;
        private readonly ITargetService _targetService;

        public DeleteTargetCommand(IBot bot, ITelegramUserService telegramUserService, ITargetService targetService)
        {
            _bot = bot;
            _telegramUserService = telegramUserService;
            _targetService = targetService;
        }

        public bool CanExecute(UpdateArgs update, TelegramUser user)
        {
            var split = (string.IsNullOrEmpty(user.ContextMenu) ? string.Empty : user.ContextMenu).Split('/');
            return split.Length == 4 && split[0] == ContextMenus.Account && split[2] == ContextMenus.Target
                && (update.GetTextData() == DEFAULT_STYLE
                || update.GetTextData() == TEXT_STYLE);
        }

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            string[] splitContextMenu = user.ContextMenu?.Split('/') ?? throw new InvalidDataException("Missing context menu");
            string text = update.GetTextData();
            if (Status == 0)
            {
                await AskForDelete(user, text, splitContextMenu);
                Status++;
            }
            else
            {
                await ProcessDeletion(user, text, splitContextMenu);
                IsFinished = true;
            }
        }

        private async Task ProcessDeletion(TelegramUser user, string text, string[] splitContextMenu)
        {
            if (text == GeneralCommands.Confirm)
            {
                string accountName = splitContextMenu[1];
                string targetName = splitContextMenu[3];
                if (user.UserId is null) throw new InvalidOperationException("User id is null");
                await _targetService.DeleteByName(user.UserId.Value, accountName, targetName);

                await _bot.Write(user, new TextMessageArgs
                {
                    Text = $"Target <code>{targetName}</code> in account <code>{accountName}</code> has been deleted"
                });

                await _telegramUserService.SetContextMenu(user, $"{ContextMenus.Account}/{accountName}/{ContextMenus.Target}");
            }
        }

        private async Task AskForDelete(TelegramUser user, string text, string[] splitContextMenu)
        {
            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Are you sure you want to delete target <code>{splitContextMenu[3]}</code> " +
                $"in account <code>{splitContextMenu[1]}</code> with ALL data?",
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
