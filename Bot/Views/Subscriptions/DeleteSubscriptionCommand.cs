using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Operations;
using FinancialAdvisorTelegramBot.Services.Telegram;
using FinancialAdvisorTelegramBot.Utils.CommandSerializing;

namespace FinancialAdvisorTelegramBot.Bot.Views.Subscriptions
{
    public class DeleteSubscriptionCommand : ICommand
    {
        public static string TEXT_STYLE => "Delete subscription";
        public static string DEFAULT_STYLE => "/delete";
        public virtual bool IsFinished { get; private set; } = false;
        public bool ShowContextMenuAfterExecution => true;

        [CommandPropertySerializable] public int Status { get; set; }

        private readonly IBot _bot;
        private readonly ITelegramUserService _telegramUserService;
        private readonly ISubscriptionService _subscriptionService;

        public DeleteSubscriptionCommand(IBot bot, ITelegramUserService telegramUserService, ISubscriptionService subscriptionService)
        {
            _bot = bot;
            _telegramUserService = telegramUserService;
            _subscriptionService = subscriptionService;
        }

        public bool CanExecute(UpdateArgs update, TelegramUser user)
        {
            var contextMenu = (string.IsNullOrEmpty(user.ContextMenu) ? string.Empty : user.ContextMenu).Split('/');
            return ((contextMenu.Length == 2 && contextMenu[0] == ContextMenus.Subscription)
                || (contextMenu.Length == 4 && contextMenu[0] == ContextMenus.Account && contextMenu[2] == ContextMenus.Subscription))
                && (update.GetTextData() == DEFAULT_STYLE || update.GetTextData() == TEXT_STYLE);
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
                string subscriptionName = splitContextMenu.Length == 4 ? splitContextMenu[3] : splitContextMenu[1];
                if (user.UserId is null) throw new InvalidOperationException("User id is null");
                await _subscriptionService.DeleteByName(user.UserId.Value, subscriptionName);

                await _bot.Write(user, new TextMessageArgs
                {
                    Text = $"Subscription <code>{subscriptionName}</code> has been deleted"
                });
                await _telegramUserService.SetContextMenu(user, splitContextMenu.Length == 4 
                    ? $"{ContextMenus.Account}/{splitContextMenu[1]}/{ContextMenus.Subscription}" 
                    : ContextMenus.Subscription);
            }
        }

        private async Task AskForDelete(TelegramUser user, string text, string[] splitContextMenu)
        {
            string subscriptionName = splitContextMenu.Length == 4 ? splitContextMenu[3] : splitContextMenu[1];
            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Are you sure you want to delete subscription <code>{subscriptionName}</code> with ALL data?",
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
