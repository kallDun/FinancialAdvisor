using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Operations;
using FinancialAdvisorTelegramBot.Services.Telegram;

namespace FinancialAdvisorTelegramBot.Bot.Views.Subscriptions
{
    public class SubscriptionByNameMenuCommand : ICommand
    {
        public static string TEXT_STYLE => "Subscription menu";
        public static string DEFAULT_STYLE => "/subscription_menu";

        private readonly IBot _bot;
        private readonly ITelegramUserService _telegramUserService;
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionByNameMenuCommand(IBot bot, ITelegramUserService telegramUserService, ISubscriptionService subscriptionService)
        {
            _bot = bot;
            _telegramUserService = telegramUserService;
            _subscriptionService = subscriptionService;
        }

        public bool IsContextMenu(string[] contextMenu)
            => (contextMenu.Length == 2 && contextMenu[0] == ContextMenus.Subscription)
            || (contextMenu.Length == 4 && contextMenu[0] == ContextMenus.Accounts && contextMenu[2] == ContextMenus.Subscription);
        
        public bool CanExecute(UpdateArgs update, TelegramUser user)
        {
            var contextMenu = (string.IsNullOrEmpty(user.ContextMenu) ? string.Empty : user.ContextMenu).Split('/');            
            return ((contextMenu.Length >= 2 && contextMenu[0] == ContextMenus.Subscription)
                || (contextMenu.Length >= 4 && contextMenu[0] == ContextMenus.Accounts && contextMenu[2] == ContextMenus.Subscription))
                && (update.GetTextData() == DEFAULT_STYLE || update.GetTextData() == TEXT_STYLE);
        }
        
        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            if (user.UserId is null) throw new InvalidDataException("Profile id cannot be null");
            var contextMenuSplit = user.ContextMenu?.Split('/') ?? throw new InvalidDataException("Missing context menu");            
            bool contextMenuWithAccount = contextMenuSplit.Length >= 4 && contextMenuSplit[2] == ContextMenus.Subscription && contextMenuSplit[0] == ContextMenus.Accounts;
            string name = contextMenuWithAccount ? contextMenuSplit[3] : contextMenuSplit[1];

            List<string> buttons = await _subscriptionService.GetByName(user.UserId.Value, name) is not null
                ? new()
                {
                    SubscriptionsMenuCommand.TEXT_STYLE
                }
                : new()
                {
                    SubscriptionsMenuCommand.TEXT_STYLE
                };

            await _telegramUserService.SetContextMenu(user,
                contextMenuWithAccount
                ? $"{ContextMenus.Accounts}/{contextMenuSplit[1]}/{ContextMenus.Subscription}/{name}"
                : $"{ContextMenus.Subscription}/{name}");

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"<b>↓ Subscription <code>{name}</code> ↓</b>",
                Placeholder = "Select command",
                MarkupType = ReplyMarkupType.ReplyKeyboard,
                ReplyKeyboardButtons = buttons,
            });
        }
    }
}
