using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Bot.Views.Accounts;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Operations;
using FinancialAdvisorTelegramBot.Services.Telegram;

namespace FinancialAdvisorTelegramBot.Bot.Views.Subscriptions
{
    public class SubscriptionsMenuCommand : ICommand
    {
        public static string TEXT_STYLE => "Subscriptions menu";
        public static string DEFAULT_STYLE => "/subscriptions_menu";

        private readonly IBot _bot;
        private readonly ITelegramUserService _telegramUserService;
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionsMenuCommand(IBot bot, ITelegramUserService telegramUserService, ISubscriptionService subscriptionService)
        {
            _bot = bot;
            _telegramUserService = telegramUserService;
            _subscriptionService = subscriptionService;
        }

        public bool IsContextMenu(string[] contextMenu)
            => (contextMenu.Length == 1 && contextMenu[0] == ContextMenus.Subscription)
            || (contextMenu.Length == 3 && contextMenu[0] == ContextMenus.Accounts && contextMenu[2] == ContextMenus.Subscription);

        public bool CanExecute(UpdateArgs update, TelegramUser user)
        {
            var split = (string.IsNullOrEmpty(user.ContextMenu) ? string.Empty : user.ContextMenu).Split('/');
            return ((split.Length >= 2 && split[0] == ContextMenus.Accounts)
                || split.Length == 1 && split[0] == ContextMenus.MainMenu
                || split.Length >= 1 && split[0] == ContextMenus.Subscription)
                && (update.GetTextData() == DEFAULT_STYLE
                || update.GetTextData() == TEXT_STYLE)
                && user.UserId is not null;
        }

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            if (user.UserId is null) throw new InvalidDataException("User id cannot be null");
            var split = user.ContextMenu?.Split('/') ?? throw new InvalidDataException("Missing context menu");
            if (!((split.Length >= 2 && split[0] == ContextMenus.Accounts)
                || split.Length >= 1 && split[0] == ContextMenus.Subscription
                || split.Length == 1 && split[0] == ContextMenus.MainMenu)) throw new InvalidDataException("Invalid context menu");

            bool contextMenuWithAccount = split.Length >= 2 && split[0] == ContextMenus.Accounts;
            bool hasAnySubscriptions = contextMenuWithAccount 
                ? await _subscriptionService.HasAny(user.UserId.Value, split[1])
                : await _subscriptionService.HasAny(user.UserId.Value);

            List<string> buttons = hasAnySubscriptions
                ? new()
                {
                    SelectSubscriptionCommand.TEXT_STYLE,
                    ViewSubscriptionsCommand.TEXT_STYLE,
                    CreateSubscriptionCommand.TEXT_STYLE
                }
                : new()
                {
                    CreateSubscriptionCommand.TEXT_STYLE
                };
            
            // add back to previous menu button
            buttons.Add(contextMenuWithAccount
                ? AccountByNameMenuCommand.TEXT_STYLE
                : MainMenuCommand.TEXT_STYLE);

            await _telegramUserService.SetContextMenu(user,
                contextMenuWithAccount
                ? $"{split[0]}/{split[1]}/{ContextMenus.Subscription}"
                : ContextMenus.Subscription);

            await _bot.Write(user, new TextMessageArgs
            {
                Text = "<b>↓ Subscription menu ↓</b>",
                Placeholder = "Select command",
                MarkupType = ReplyMarkupType.ReplyKeyboard,
                ReplyKeyboardButtons = buttons,
            });
        }
    }
}
