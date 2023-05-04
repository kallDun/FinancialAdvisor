using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Operations;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Operations;

namespace FinancialAdvisorTelegramBot.Bot.Views.Subscriptions
{
    public class ViewSubscriptionCommand : ICommand
    {
        public static string TEXT_STYLE => "View subscription";
        public static string DEFAULT_STYLE => "/view";

        private readonly IBot _bot;
        private readonly ISubscriptionService _subscriptionService;

        public ViewSubscriptionCommand(IBot bot, ISubscriptionService subscriptionService)
        {
            _bot = bot;
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
            var splitContextMenu = user.ContextMenu?.Split('/') ?? throw new InvalidDataException("Missing context menu");
            string subscriptionName = splitContextMenu[1];

            Subscription subscription = await _subscriptionService.GetByName(user.UserId
                ?? throw new InvalidDataException("User id cannot be null"), subscriptionName, loadAllData: true)
                ?? throw new InvalidDataException($"Cannot find category with name {subscriptionName}");

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"<u><b>Subscription {subscription.Name}:</b></u>\n" +
                $"\nAmount: <code>{subscription.Amount}</code>" +
                $"\nNext payment date: <code>{subscription.NextPaymentDate:dd.MM.yyyy}</code>" +
                $"\nPayment day: <code>{subscription.PaymentDay}</code>" +
                $"\nCategory: <code>{subscription.Category?.Name}</code>" +
                $"\nAuto pay function: <code>{(subscription.AutoPay ? "enabled" : "disabled")}</code>" +
                $"\nOverdue payments count: <code>{subscription.OverduePaymentNumber}</code>" +
                $"{(subscription.Account is null ? "" : $"\nAccount owner: <code>{subscription.Account.Name}</code>")}"
            });
        }
    }
}
