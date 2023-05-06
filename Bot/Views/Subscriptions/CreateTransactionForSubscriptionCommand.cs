using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Operations;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;
using FinancialAdvisorTelegramBot.Services.Operations;
using FinancialAdvisorTelegramBot.Utils;
using FinancialAdvisorTelegramBot.Utils.CommandSerializing;

namespace FinancialAdvisorTelegramBot.Bot.Views.Subscriptions
{
    public class CreateTransactionForSubscriptionCommand : ICommand
    {
        private enum CreatingTransactionStatus
        {
            AskAccount, AskType, AskTransactionTime, AskLimitConfirmation, Finished
        }
        
        private const string TimeNowCommand = "/now";
        private const string ConfirmCommand = "/confirm";

        public static string TEXT_STYLE => "Create new transaction";
        public static string DEFAULT_STYLE => "/transaction";
        public bool IsFinished { get; private set; } = false;
        public bool ShowContextMenuAfterExecution => true;

        private CreatingTransactionStatus _status => (CreatingTransactionStatus)Status;
        private SubscriptionTransactionType _type => (SubscriptionTransactionType)Type;
        [CommandPropertySerializable] public string? AccountName { get; set; }
        [CommandPropertySerializable] public int Status { get; set; }
        [CommandPropertySerializable] public int Type { get; set; }
        [CommandPropertySerializable] public DateTime TransactionTime { get; set; }

        private readonly IBot _bot;
        private readonly ILimitByCategoryService _limitByCategoryService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly IAccountService _accountService;

        public CreateTransactionForSubscriptionCommand(IBot bot, ILimitByCategoryService limitByCategoryService, 
            ISubscriptionService subscriptionService, IAccountService accountService)
        {
            _bot = bot;
            _limitByCategoryService = limitByCategoryService;
            _subscriptionService = subscriptionService;
            _accountService = accountService;
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
            if (user.UserId is null) throw new InvalidDataException("User id cannot be null");
            var splitContextMenu = user.ContextMenu?.Split('/') ?? throw new InvalidDataException("Missing context menu");

            string text = update.GetTextData();
            Task function = _status switch
            {
                CreatingTransactionStatus.AskAccount => AskAccount(user, text, splitContextMenu),
                CreatingTransactionStatus.AskType => AskType(user, text, splitContextMenu),
                CreatingTransactionStatus.AskTransactionTime => AskTransactionTime(user, text, splitContextMenu),
                CreatingTransactionStatus.AskLimitConfirmation => AskLimitConfirmation(user, text, splitContextMenu),
                CreatingTransactionStatus.Finished => ProcessResult(user, text, splitContextMenu),
                _ => throw new InvalidDataException("Invalid status")
            };
            await function;

            if (_status is CreatingTransactionStatus.Finished)
            {
                IsFinished = true;
            }
            Status++;
        }

        private async Task ProcessResult(TelegramUser telegramUser, string text, string[] splitContextMenu)
        {
            if (text != ConfirmCommand) throw new ArgumentException("User cancel transaction");

            string subscriptionName = splitContextMenu.Length == 4 ? splitContextMenu[3] : splitContextMenu[1];
            Subscription subscription = await _subscriptionService.GetByName(telegramUser.UserId
                ?? throw new InvalidDataException("User id cannot be null"), subscriptionName, loadAllData: true)
                ?? throw new InvalidDataException("Subscription not found");

            Account account = (await _accountService.GetByName(telegramUser.UserId
                ?? throw new InvalidDataException("User id cannot be null"), AccountName
                ?? throw new InvalidDataException("Account name cannot be null")))
                ?? throw new InvalidDataException("Account not found");
            
            Transaction? transaction = await _subscriptionService.CreateTransaction(subscription, account, TransactionTime, _type);

            await _bot.Write(telegramUser, new TextMessageArgs
            {
                Text = _type switch
                {
                    SubscriptionTransactionType.Default => $"Transaction to account {account.Name} was created successfully." +
                    $"\n<code>Next payment day is {subscription.NextPaymentDate:dd.MM.yyyy}</code> with <code>{subscription.Amount}</code>",
                    SubscriptionTransactionType.Late => $"Late transaction to account {account.Name} was created successfully." +
                    $"\n<code>Next payment day is still {subscription.NextPaymentDate:dd.MM.yyyy}</code> with <code>{subscription.Amount}</code>" +
                    $"\nCount of overdue payments was decreased by 1 and now is <code>{subscription.OverduePaymentNumber}</code>",
                    SubscriptionTransactionType.Delayed => $"Transaction was skipped." +
                    $"\n<code>Next payment day is {subscription.NextPaymentDate:dd.MM.yyyy}</code> with <code>{subscription.Amount}</code>" +
                    $"\nCount of overdue payments was increased by 1 and now is <code>{subscription.OverduePaymentNumber}</code>",
                    _ => throw new InvalidDataException("Unknown transaction type")
                }
            });
        }

        private async Task AskLimitConfirmation(TelegramUser user, string text, string[] splitContextMenu)
        {
            TransactionTime = text == TimeNowCommand
                ? DateTime.Now
                : Converters.ToDateTime(text);

            string subscriptionName = splitContextMenu.Length == 4 ? splitContextMenu[3] : splitContextMenu[1];
            Subscription subscription = await _subscriptionService.GetByName(user.UserId
                ?? throw new InvalidDataException("User id cannot be null"), subscriptionName, loadAllData: true)
                ?? throw new InvalidDataException("Subscription not found");

            if (subscription.Amount < 0 && await _limitByCategoryService.IsTransactionExceedLimit(
                subscription.User ?? throw new InvalidDataException("User cannot be null"),
                subscription.Category?.Name ?? throw new InvalidDataException("Category cannot be null"),
                Math.Abs(subscription.Amount), TransactionTime))
            {
                await _bot.Write(user, new TextMessageArgs
                {
                    Text = $"Confirm transaction that exceeds category limit:",
                    MarkupType = ReplyMarkupType.InlineKeyboard,
                    InlineKeyboardButtons = new List<List<InlineButton>>()
                    {
                        new() { new("Cancel", GeneralCommands.Cancel), new("Confirm", ConfirmCommand) }
                    }
                });
            }
            else
            {
                Status++;
                await ProcessResult(user, ConfirmCommand, splitContextMenu);
            }
        }

        private async Task AskTransactionTime(TelegramUser user, string text, string[] splitContextMenu)
        {
            if (int.TryParse(text, out int type))
            {
                if (type >= 0 && type <= 2) Type = type;
                else throw new ArgumentException("Invalid transaction type");
            }
            else throw new ArgumentException("Invalid transaction type");

            if (_type is SubscriptionTransactionType.Delayed)
            {
                Status += 2;
                await ProcessResult(user, ConfirmCommand, splitContextMenu);
                return;
            }

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Write transaction time:" +
                $"\n<code>(time format '{Converters.DateTimeFormat}')</code>",
                Placeholder = "Write time",
                MarkupType = ReplyMarkupType.InlineKeyboard,
                InlineKeyboardButtons = new() { new() { new("Set transaction time to now", TimeNowCommand) } }
            });
        }

        private async Task AskType(TelegramUser user, string text, string[] splitContextMenu)
        {
            Account account = await _accountService.GetByName(user.UserId 
                ?? throw new InvalidDataException("User id cannot be null"), text.Trim()) 
                ?? throw new ArgumentNullException("Account not found");
            AccountName = account.Name;
            
            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Write transaction type:",
                Placeholder = "Select type",
                MarkupType = ReplyMarkupType.InlineKeyboard,
                InlineKeyboardButtons = new List<List<InlineButton>>()
                {
                    new() { new($"{SubscriptionTransactionType.Default}", ((int)SubscriptionTransactionType.Default).ToString()) },
                    new() { new($"{SubscriptionTransactionType.Late}",((int)SubscriptionTransactionType.Late).ToString()) },
                    new() { new($"{SubscriptionTransactionType.Delayed}", ((int)SubscriptionTransactionType.Delayed).ToString()) }
                }
            });
        }

        private async Task AskAccount(TelegramUser user, string text, string[] splitContextMenu)
        {
            string subscriptionName = splitContextMenu.Length == 4 ? splitContextMenu[3] : splitContextMenu[1];
            Subscription subscription = await _subscriptionService.GetByName(user.UserId
                ?? throw new InvalidDataException("User id cannot be null"), subscriptionName, loadAllData: false)
                ?? throw new InvalidDataException("Subscription not found");

            if (splitContextMenu.Length == 4)
            {
                Status++;
                await AskType(user, splitContextMenu[1], splitContextMenu);
                return;
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
                Text = "<b>↓ Select account ↓</b>",
                Placeholder = "Select",
                MarkupType = ReplyMarkupType.InlineKeyboard,
                InlineKeyboardButtons = inlineButtons
            });
        }
    }
}
