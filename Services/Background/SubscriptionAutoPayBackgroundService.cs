using FinancialAdvisorTelegramBot.Bot;
using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Views.Subscriptions;
using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Services.Operations;
using FinancialAdvisorTelegramBot.Utils.Attributes;

namespace FinancialAdvisorTelegramBot.Services.Background
{
    [CustomBackgroundService(days: 1)]
    public class SubscriptionAutoPayBackgroundService : IHostedService
    {
        private readonly IBot _bot;
        private readonly ISubscriptionService _subscriptionService;
        private readonly ILimitByCategoryService _limitByCategoryService;

        public SubscriptionAutoPayBackgroundService(IBot bot, ISubscriptionService subscriptionService, ILimitByCategoryService limitByCategoryService)
        {
            _bot = bot;
            _subscriptionService = subscriptionService;
            _limitByCategoryService = limitByCategoryService;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var subscriptions = await _subscriptionService.GetAllWithData();
            foreach (var subscription in subscriptions)
            {
                bool flag = false;
                string message = "<b>[AutoPay Service]:</b> ";
                try
                {
                    if (subscription.NextPaymentDate == DateTime.Now.Date)
                    {
                        flag = true;
                        if (subscription.AutoPay)
                        {
                            DateTime transactionTime = DateTime.Now;

                            if (subscription.Amount < 0 &&
                                await _limitByCategoryService.IsTransactionExceedLimit(subscription.User 
                                ?? throw new InvalidDataException("User cannot be null"), subscription.Category?.Name
                                ?? throw new InvalidDataException("Category cannot be null"), Math.Abs(subscription.Amount), transactionTime))
                            {
                                throw new ArgumentException("Transaction exceeds limit");
                            }

                            Transaction transaction = await _subscriptionService.CreateTransaction(subscription, transactionTime, SubscriptionTransactionType.Default)
                                ?? throw new InvalidDataException("Transaction was not created");
                            message += transaction.Amount > 0
                                ? $"Income from subscription <code>{subscription.Name}</code> was received. Amount: <code>{transaction.Amount}</code>"
                                : $"Subscription <code>{subscription.Name}</code> was paid. Amount: <code>{Math.Abs(transaction.Amount)}</code>";
                        }
                        else
                        {
                            message += subscription.Amount > 0
                                ? $"It's time to get <code>{subscription.Amount}</code> income from subscription <code>{subscription.Name}</code>."
                                : $"It's time to pay <code>{Math.Abs(subscription.Amount)}</code> for subscription <code>{subscription.Name}</code>.";
                            message += $" To create transaction, go to subscriptions menu: {SubscriptionsMenuCommand.DEFAULT_STYLE}";
                        }
                    }
                }
                catch (Exception e)
                {
                    message += $"Error occurred while processing subscription <code>{subscription.Name}</code>.\n<b>Error:</b> {e.Message}." +
                        $"\nTo create transaction manually, go to subscriptions menu: {SubscriptionsMenuCommand.DEFAULT_STYLE}";
                }
                if (flag)
                {
                    await _bot.Write(subscription?.User?.TelegramUser
                        ?? throw new InvalidDataException("There is not telegram user in subscription entity"),
                        new TextMessageArgs() { Text = message });
                }
            }
        }
    }
}
