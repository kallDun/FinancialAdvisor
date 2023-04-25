using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Operations;
using FinancialAdvisorTelegramBot.Services.Telegram;
using FinancialAdvisorTelegramBot.Utils.CommandSerializing;

namespace FinancialAdvisorTelegramBot.Bot.Views.Subscriptions
{
    public class SelectSubscriptionCommand : ICommand
    {
        public static string TEXT_STYLE => "Select subscription";
        public static string DEFAULT_STYLE => "/select";
        public bool IsFinished { get; private set; } = false;

        [CommandPropertySerializable] public int Status { get; set; }

        private readonly IBot _bot;
        private readonly ITelegramUserService _telegramUserService;
        private readonly ISubscriptionService _subscriptionService;
        
        public SelectSubscriptionCommand(IBot bot, ITelegramUserService telegramUserService, ISubscriptionService subscriptionService)
        {
            _bot = bot;
            _telegramUserService = telegramUserService;
            _subscriptionService = subscriptionService;
        }

        public bool ShowContextMenuAfterExecution => true;

        public bool CanExecute(UpdateArgs update, TelegramUser user)
        {
            var contextMenuSplit = (string.IsNullOrEmpty(user.ContextMenu) ? string.Empty : user.ContextMenu).Split('/');
            return ((contextMenuSplit.Length == 3 && contextMenuSplit[2] == ContextMenus.Subscription)
                || contextMenuSplit.Length == 1 && contextMenuSplit[0] == ContextMenus.Subscription)
                && (update.GetTextData() == DEFAULT_STYLE
                || update.GetTextData() == TEXT_STYLE);
        }

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            var contextMenuSplit = user.ContextMenu?.Split('/') ?? throw new InvalidDataException("Missing context menu");
            if (Status == 0)
            {
                await AskSubscriptionName(user, contextMenuSplit);
                Status++;
            }
            else
            {
                await SetSubscriptionContextMenu(update, user);
                IsFinished = true;
            }
        }
        
        private async Task SetSubscriptionContextMenu(UpdateArgs update, TelegramUser user)
        {
            if (user.UserId is null) throw new InvalidDataException("Profile id cannot be null");
            string name = update.GetTextData().Trim();
            var subscription = await _subscriptionService.GetByName(user.UserId.Value, name);
            if (subscription is null) throw new ArgumentNullException("Subscription not found");

            await _telegramUserService.SetContextMenu(user, $"{user.ContextMenu}/{name}");
        }

        private async Task AskSubscriptionName(TelegramUser user, string[] contextMenuSplit)
        {
            if (user.UserId is null) throw new InvalidDataException("Profile id cannot be null");
            bool contextMenuWithAccount = contextMenuSplit.Length == 3 && contextMenuSplit[0] == ContextMenus.Accounts;
            string? accountName = contextMenuWithAccount ? contextMenuSplit[1] : null;

            var subscriptions = await _subscriptionService.LoadAllWithAccounts(user.UserId.Value, accountName);

            await _bot.Write(user, new TextMessageArgs
            {
                Text = "<b>↓ Available subscriptions ↓</b>",
                Placeholder = "Select command",
                MarkupType = ReplyMarkupType.InlineKeyboard,
                InlineKeyboardButtons = subscriptions
                .Select(account => new List<InlineButton>() { new InlineButton(
                        account.Name ?? "--Empty name--",
                        account.Name ?? GeneralCommands.Cancel) })
                .ToList()
            });
        }
    }
}
