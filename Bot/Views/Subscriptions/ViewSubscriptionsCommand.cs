using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Operations;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Operations;

namespace FinancialAdvisorTelegramBot.Bot.Views.Subscriptions
{
    public class ViewSubscriptionsCommand : ICommand
    {
        public static string TEXT_STYLE => "View subscriptions";
        public static string DEFAULT_STYLE => "/view";

        private readonly IBot _bot;
        private readonly ISubscriptionService _subscriptionService;

        public ViewSubscriptionsCommand(IBot bot, ISubscriptionService subscriptionService)
        {
            _bot = bot;
            _subscriptionService = subscriptionService;
        }

        public bool CanExecute(UpdateArgs update, TelegramUser user)
        {
            var split = (string.IsNullOrEmpty(user.ContextMenu) ? string.Empty : user.ContextMenu).Split('/');
            return ((split.Length >= 2 && split[0] == ContextMenus.Accounts)
                || split.Length == 1 && split[0] == ContextMenus.Subscription)
                && (update.GetTextData() == DEFAULT_STYLE
                || update.GetTextData() == TEXT_STYLE);
        }

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            if (user.UserId is null) throw new InvalidDataException("User id cannot be null");
            var split = user.ContextMenu?.Split('/') ?? throw new InvalidDataException("Missing context menu");
            if (!((split.Length == 3 && split[2] == ContextMenus.Subscription)
                || split.Length == 1 && split[0] == ContextMenus.Subscription)) throw new InvalidDataException("Invalid context menu");

            IList<Subscription> subscriptions = await _subscriptionService.LoadAllWithAccounts(user.UserId.Value,
                split.Length == 3 ? split[1] : null);

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"<u><b>Subscriptions:</b></u>\n" +
                string.Join("\n-------------------------------", subscriptions.Select(subscription =>
                    $"\nName: <code>{subscription.Name}</code>" +
                    $"\nNext payment date: " +
                    $"<code>{_subscriptionService.GetNextPaymentDate(subscription.PaymentDay, subscription.LastPaymentDate ?? DateTime.Now):dd/MM/yyyy}</code>" +
                    $"\nAmount: <code>{subscription.Amount}</code>" +
                    $"{(subscription.Account is null ? "" 
                        : $"\nAccount owner: <code>{subscription.Account.Name}</code>")}"
                ))
            });
        }
    }
}
