using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;
using FinancialAdvisorTelegramBot.Services.Operations;
using FinancialAdvisorTelegramBot.Utils;
using FinancialAdvisorTelegramBot.Utils.CommandSerializing;

namespace FinancialAdvisorTelegramBot.Bot.Views.Transactions
{
    public class CreateTransferToAccountCommand : ICommand
    {
        private enum CreatingTransferStatus
        {
            AskAccountTo, AskAmount, AskTransactionTime, AskCategory, AskLimitConfirmation, AskDetails, Finished
        }

        private const string TimeNowCommand = "/now";

        public static string TEXT_STYLE => "Create new transfer";
        public static string DEFAULT_STYLE => "/transfer";
        public bool IsFinished { get; private set; } = false;
        public bool ShowContextMenuAfterExecution => true;

        private CreatingTransferStatus _status => (CreatingTransferStatus)Status;
        [CommandPropertySerializable] public int Status { get; set; }
        [CommandPropertySerializable] public int AccountRecipientId { get; set; }
        [CommandPropertySerializable] public decimal Amount { get; set; }
        [CommandPropertySerializable] public DateTime TransactionTime { get; set; }
        [CommandPropertySerializable] public int CategoryId { get; set; }
        [CommandPropertySerializable] public string? Details { get; set; }

        private readonly IBot _bot;
        private readonly IUserService _userService;
        private readonly IAccountService _accountService;
        private readonly ICategoryService _categoryService;
        private readonly ILimitByCategoryService _limitByCategoryService;

        public CreateTransferToAccountCommand(IBot bot, IAccountService accountService, IUserService userService, 
            ICategoryService categoryService, ILimitByCategoryService limitByCategoryService)
        {
            _bot = bot;
            _accountService = accountService;
            _userService = userService;
            _categoryService = categoryService;
            _limitByCategoryService = limitByCategoryService;
        }

        public bool CanExecute(UpdateArgs update, TelegramUser user)
        {
            var splitContextMenu = (string.IsNullOrEmpty(user.ContextMenu) ? string.Empty : user.ContextMenu).Split('/');
            return (splitContextMenu.Length == 3
                && splitContextMenu[0] == ContextMenus.Account
                && splitContextMenu[2] == ContextMenus.Transaction)
                && (update.GetTextData() == DEFAULT_STYLE
                || update.GetTextData() == TEXT_STYLE)
                && user.UserId is not null;
        }

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            var splitContextMenu = user.ContextMenu?.Split('/') ?? throw new InvalidDataException("Missing context menu");
            string text = update.GetTextData();

            Task function = _status switch
            {
                CreatingTransferStatus.AskAccountTo => AskAccountTo(user, text, splitContextMenu),
                CreatingTransferStatus.AskAmount => AskAmount(user, text, splitContextMenu),
                CreatingTransferStatus.AskTransactionTime => AskTransactionTime(user, text, splitContextMenu),
                CreatingTransferStatus.AskCategory => AskCategory(user, text, splitContextMenu),
                CreatingTransferStatus.AskLimitConfirmation => AskLimitConfirmation(user, text, splitContextMenu),
                CreatingTransferStatus.AskDetails => AskDetails(user, text, splitContextMenu),
                CreatingTransferStatus.Finished => ProcessResult(user, text, splitContextMenu),
                _ => throw new InvalidDataException("Invalid status")
            };
            await function;

            if (_status is CreatingTransferStatus.Finished)
            {
                IsFinished = true;
            }
            Status++;
        }

        private async Task ProcessResult(TelegramUser telegramUser, string text, string[] splitContextMenu)
        {
            Details = text == GeneralCommands.SetEmpty ? null : text;

            User user = await _userService.GetById(telegramUser.UserId
                ?? throw new InvalidDataException("User id cannot be null"))
                ?? throw new InvalidDataException("User not found");

            Account currentAccount = (await _accountService.GetByName(telegramUser.UserId.Value, splitContextMenu[1])
                ?? throw new InvalidDataException("Account not found"));

            Account accountRecipient = (await _accountService.GetById(AccountRecipientId)
                ?? throw new InvalidDataException("Account not found"));
            
            var (transactionFrom, transactionTo) = await _accountService.Transfer(user, currentAccount, accountRecipient, Amount, CategoryId, TransactionTime, Details);

            await _bot.Write(telegramUser, new TextMessageArgs
            {
                Text = $"Transfer has been created successfully." +
                $"\n(Sent <code>{Math.Abs(transactionFrom.Amount)}</code> " +
                $"to account <code>{transactionFrom.Communicator}</code>)"
            });
        }

        private async Task AskDetails(TelegramUser user, string text, string[] splitContextMenu)
        {
            if (text != GeneralCommands.Confirm) throw new ArgumentException("User cancel transaction");

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Write transaction details:",
                Placeholder = "Write details",
                MarkupType = ReplyMarkupType.InlineKeyboard,
                InlineKeyboardButtons = new() { new() { new("Set details empty", GeneralCommands.SetEmpty) } }
            });
        }

        private async Task AskLimitConfirmation(TelegramUser user, string text, string[] splitContextMenu)
        {
            User profile = await _userService.GetById(user.UserId
               ?? throw new InvalidDataException("User id cannot be null"))
               ?? throw new InvalidDataException("Profile not found");

            string categoryName = text.Trim();
            CategoryId = (await _categoryService.GetByName(user.UserId.Value, categoryName)
                ?? throw new InvalidDataException($"Cannot find category with name {categoryName}")).Id;

            if (await _limitByCategoryService.IsTransactionExceedLimit(profile, categoryName, Amount, TransactionTime))
            {
                await _bot.Write(user, new TextMessageArgs
                {
                    Text = $"Confirm transaction that exceeds category limit:",
                    MarkupType = ReplyMarkupType.InlineKeyboard,
                    InlineKeyboardButtons = new List<List<InlineButton>>()
                    {
                        new() { new("Cancel", GeneralCommands.Cancel), new("Confirm", GeneralCommands.Confirm) }
                    }
                });
            }
            else
            {
                Status++;
                await AskDetails(user, GeneralCommands.Confirm, splitContextMenu);
            }
        }

        private async Task AskCategory(TelegramUser user, string text, string[] splitContextMenu)
        {
            TransactionTime = text == TimeNowCommand
                ? DateTime.Now
                : Converters.ToDateTime(text);

            var categories = await _categoryService.GetAll(user.UserId
                ?? throw new InvalidDataException("User id cannot be null"));
            if (categories.Count == 0)
            {
                var defaultCategory = await _categoryService.GetOrOtherwiseCreateCategory(user.UserId.Value, CategoryNames.Default);
                categories.Add(defaultCategory);
            }

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Select category for transaction:",
                Placeholder = "Select category",
                MarkupType = ReplyMarkupType.InlineKeyboard,
                InlineKeyboardButtons = categories.Select(x => new List<InlineButton>()
                {
                    new(x.Name ?? "-- Missing name --",
                        x.Name ?? GeneralCommands.Cancel)
                }).ToList()
            });
        }

        private async Task AskTransactionTime(TelegramUser user, string text, string[] splitContextMenu)
        {
            Amount = Converters.ToDecimal(text);
            if (Amount <= 0) throw new ArgumentException("Amount must be more than 0");

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Write transaction time:" +
                $"\n<code>(time format '{Converters.DateTimeFormat}')</code>",
                Placeholder = "Write time",
                MarkupType = ReplyMarkupType.InlineKeyboard,
                InlineKeyboardButtons = new() { new() { new("Set transaction time to now", TimeNowCommand) } }
            });
        }

        private async Task AskAmount(TelegramUser user, string text, string[] splitContextMenu)
        {
            if (user.UserId is null) throw new InvalidDataException("User id cannot be null");
            string accountName = text.Trim();
            var account = await _accountService.GetByName(user.UserId.Value, accountName);
            if (account is null) throw new ArgumentNullException("Account not found");
            AccountRecipientId = account.Id;

            await _bot.Write(user, new TextMessageArgs
            {
                Text = "Enter positive amount:"
            });
        }

        private async Task AskAccountTo(TelegramUser user, string text, string[] splitContextMenu)
        {
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
                Text = "<b>↓ Select account recipient ↓</b>",
                Placeholder = "Select",
                MarkupType = ReplyMarkupType.InlineKeyboard,
                InlineKeyboardButtons = inlineButtons
            });
        }
    }
}
