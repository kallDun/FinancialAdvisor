using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;
using FinancialAdvisorTelegramBot.Services.Operations;
using FinancialAdvisorTelegramBot.Utils;
using FinancialAdvisorTelegramBot.Utils.CommandSerializing;

namespace FinancialAdvisorTelegramBot.Bot.Views.Subscriptions
{
    public class CreateSubscriptionCommand : ICommand
    {
        private enum CreatingSubscriptionStatus
        {
            AskAccountName, AskName, AskAmount, AskPayday, Finished
        }

        public static string TEXT_STYLE => "Create new subscription";
        public static string DEFAULT_STYLE => "/create";
        public bool IsFinished { get; private set; } = false;
        public bool ShowContextMenuAfterExecution => true;

        private readonly IBot _bot;
        private readonly IAccountService _accountService;
        private readonly ISubscriptionService _subscriptionService;

        private CreatingSubscriptionStatus _status => (CreatingSubscriptionStatus)Status;
        [CommandPropertySerializable] public int Status { get; set; }
        [CommandPropertySerializable] public string? AccountName { get; set; }
        [CommandPropertySerializable] public string? Name { get; set; }
        [CommandPropertySerializable] public decimal Amount { get; set; }
        [CommandPropertySerializable] public byte Payoutday { get; set; }

        public CreateSubscriptionCommand(IBot bot, IAccountService accountService, ISubscriptionService subscriptionService)
        {
            _bot = bot;
            _accountService = accountService;
            _subscriptionService = subscriptionService;
        }

        public bool CanExecute(UpdateArgs update, TelegramUser user)
        {
            var split = (string.IsNullOrEmpty(user.ContextMenu) ? string.Empty : user.ContextMenu).Split('/');
            return ((split.Length == 3 && split[2] == ContextMenus.Subscription)
                || split.Length == 1 && split[0] == ContextMenus.Subscription)
                && (update.GetTextData() == DEFAULT_STYLE
                || update.GetTextData() == TEXT_STYLE);
        }

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            if (user.UserId is null) throw new InvalidDataException("Profile id cannot be null");
            var split = user.ContextMenu?.Split('/') ?? throw new InvalidDataException("Missing context menu");
            if (!((split.Length == 3 && split[2] == ContextMenus.Subscription)
                || split.Length == 1 && split[0] == ContextMenus.Subscription)) throw new InvalidDataException("Invalid context menu");

            switch (_status)
            {
                case CreatingSubscriptionStatus.AskAccountName:

                    if (split.Length == 3)
                    {
                        AccountName = split[1];
                        await _bot.Write(user, new TextMessageArgs
                        {
                            Text = $"Subscription will be created in account {AccountName}"
                        });
                        Status++;
                        goto case CreatingSubscriptionStatus.AskName;
                    }

                    var accounts = await _accountService.GetByUser(user.UserId
                        ?? throw new InvalidDataException("User id is null"));
                    var inlineButtons = accounts
                        .Select(account => new List<InlineButton>() { new InlineButton(
                            account.Name ?? "--Empty name--",
                            account.Name ?? GeneralCommands.Cancel) })
                        .ToList();
                    inlineButtons.Add(new() { new("Without account", GeneralCommands.SetEmpty) });

                    await _bot.Write(user, new TextMessageArgs
                    {
                        Text = "<b>↓ Available accounts ↓</b>",
                        Placeholder = "Select command",
                        MarkupType = ReplyMarkupType.InlineKeyboard,
                        InlineKeyboardButtons = inlineButtons
                    });
                    Status++;
                    break;
                    
                    
                case CreatingSubscriptionStatus.AskName:

                    if (string.IsNullOrEmpty(AccountName) && update.GetTextData() != GeneralCommands.SetEmpty)
                    {
                        string accountName = update.GetTextData().Trim();
                        var account = await _accountService.GetByName(user.UserId
                            ?? throw new InvalidDataException("User id is null"), accountName);
                        if (account is null) throw new ArgumentNullException("Account not found");
                        AccountName = accountName;
                    }

                    await _bot.Write(user, new TextMessageArgs
                    {
                        Text = $"Write subscription name:"
                    });
                    Status++;
                    break;
                    
                    
                case CreatingSubscriptionStatus.AskAmount:

                    string name = update.GetTextData().Trim();
                    Validators.ValidateLongName(name);
                    Name = name;

                    await _bot.Write(user, new TextMessageArgs
                    {
                        Text = $"Write positive <i>(income)</i> or negative <i>(expense)</i> amount per month:"
                    });
                    Status++;
                    break;
                    
                    
                case CreatingSubscriptionStatus.AskPayday:

                    Amount = Converters.ToDecimal(update.GetTextData().Trim());
                    await _bot.Write(user, new TextMessageArgs
                    {
                        Text = $"Write payout day <i>(from 1 to 31)</i>:"
                    });
                    Status++;
                    break;
                    
                    
                case CreatingSubscriptionStatus.Finished:

                    Payoutday = Converters.ToByte(update.GetTextData().Trim());
                    
                    if (await _accountService.HasAny(user.UserId.Value) == false) throw new InvalidDataException("User has no accounts");
                    int? accountId = string.IsNullOrEmpty(AccountName) ? null
                        : (await _accountService.GetByName((int)user.UserId, AccountName) 
                        ?? throw new InvalidDataException($"Cannot find account with name {AccountName}"))
                        .Id;
                    if (Name is null) throw new InvalidDataException("Name cannot be null");

                    var subscription = await _subscriptionService.Create((int)user.UserId, accountId, Name, Amount, Payoutday);
                    await _bot.Write(user, new TextMessageArgs
                    {
                        Text = $"{Name} subscription has been created.\n" +
                        $"<code>Next payment day is " +
                        $"{_subscriptionService.GetNextPaymentDate(subscription.PaymentDay, subscription.LastPaymentDate ?? DateTime.Now):dd/MM/yyyy}"
                        + $" with {subscription.Amount}</code>"
                    });

                    IsFinished = true;
                    break;
            }
        }
    }
}
