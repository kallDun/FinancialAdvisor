using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;
using FinancialAdvisorTelegramBot.Services.Operations;
using FinancialAdvisorTelegramBot.Utils;
using FinancialAdvisorTelegramBot.Utils.CommandSerializing;

namespace FinancialAdvisorTelegramBot.Bot.Views.Targets
{
    public class CreateTargetTransactionCommand : ICommand
    {
        private enum CreatingTransactionStatus
        {
            AskType, AskAmount, AskCategory, AskLimitConfirmation, Finished
        }

        private const string IncomeCommand = "/income";
        private const string ExpenseCommand = "/expense";
        private const string ConfirmCommand = "/confirm";

        public static string TEXT_STYLE => "Create transaction";
        public static string DEFAULT_STYLE => "/transaction";
        public bool IsFinished { get; private set; } = false;
        public bool ShowContextMenuAfterExecution => true;

        private CreatingTransactionStatus _status => (CreatingTransactionStatus)Status;
        [CommandPropertySerializable] public int Status { get; set; }
        [CommandPropertySerializable] public bool IsIncomeType { get; set; }
        [CommandPropertySerializable] public decimal Amount { get; set; }
        [CommandPropertySerializable] public int CategoryId { get; set; }
        [CommandPropertySerializable] public string? Details { get; set; }
        [CommandPropertySerializable] public DateTime TransactionTime { get; set; }

        private readonly IBot _bot;
        private readonly IAccountService _accountService;
        private readonly IUserService _userService;
        private readonly ICategoryService _categoryService;
        private readonly ILimitByCategoryService _limitByCategoryService;
        private readonly ITargetService _targetService;

        public CreateTargetTransactionCommand(IBot bot, IAccountService accountService, IUserService userService, ICategoryService categoryService, 
            ILimitByCategoryService limitByCategoryService, ITargetService targetService)
        {
            _bot = bot;
            _accountService = accountService;
            _userService = userService;
            _categoryService = categoryService;
            _limitByCategoryService = limitByCategoryService;
            _targetService = targetService;
        }

        public bool CanExecute(UpdateArgs update, TelegramUser user)
        {
            var split = (string.IsNullOrEmpty(user.ContextMenu) ? string.Empty : user.ContextMenu).Split('/');
            return split.Length == 4 && split[0] == ContextMenus.Account && split[2] == ContextMenus.Target
                && (update.GetTextData() == DEFAULT_STYLE
                || update.GetTextData() == TEXT_STYLE);
        }

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            if (user.UserId is null) throw new InvalidDataException("User id cannot be null");
            var splitContextMenu = user.ContextMenu?.Split('/') ?? throw new InvalidDataException("Missing context menu");

            string text = update.GetTextData();
            Task function = _status switch
            {
                CreatingTransactionStatus.AskType => AskType(user, text, splitContextMenu),
                CreatingTransactionStatus.AskAmount => AskAmount(user, text, splitContextMenu),
                CreatingTransactionStatus.AskCategory => AskCategory(user, text, splitContextMenu),
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

            string accountName = splitContextMenu[1];
            string targetName = splitContextMenu[3];

            int accountId = (await _accountService.GetByName(telegramUser.UserId
                ?? throw new InvalidDataException("User id cannot be null"), accountName)
                ?? throw new InvalidDataException("Account not found")).Id;

            User user = await _userService.GetById(telegramUser.UserId.Value)
                ?? throw new InvalidDataException("User not found");

            Transaction transaction = await _targetService.CreateTransaction(user, targetName, IsIncomeType ? Amount : -Amount, accountId, CategoryId, TransactionTime);

            await _bot.Write(telegramUser, new TextMessageArgs
            {
                Text = $"Transaction has been created successfully." +
                $"\n(Sent <code>{Math.Abs(transaction.Amount)}</code> " +
                $"{(IsIncomeType ? "to" : "from")} target account <code>{targetName}</code> " +
                $"{(IsIncomeType ? "from" : "to")} account <code>{accountName}</code>)"
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

            TransactionTime = DateTime.Now;
            if (!IsIncomeType && await _limitByCategoryService.IsTransactionExceedLimit(profile, categoryName, Amount, TransactionTime))
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

        private async Task AskCategory(TelegramUser user, string text, string[] splitContextMenu)
        {
            Amount = Converters.ToDecimal(text);
            if (Amount <= 0) throw new ArgumentException("Amount must be more than 0");

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

        private async Task AskAmount(TelegramUser user, string text, string[] splitContextMenu)
        {
            IsIncomeType = text == IncomeCommand ? true
                : text == ExpenseCommand ? false
                : throw new InvalidDataException("Invalid type");

            await _bot.Write(user, new TextMessageArgs
            {
                Text = "Enter amount:"
            });
        }

        private async Task AskType(TelegramUser user, string text, string[] splitContextMenu)
        {
            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Write transaction type:",
                Placeholder = "Select type",
                MarkupType = ReplyMarkupType.InlineKeyboard,
                InlineKeyboardButtons = new List<List<InlineButton>>()
                {
                    new() { new("Expense", ExpenseCommand), new("Income", IncomeCommand) }
                }
            });
        }
    }
}
