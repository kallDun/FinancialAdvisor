﻿using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;
using FinancialAdvisorTelegramBot.Utils;
using FinancialAdvisorTelegramBot.Utils.CommandSerializing;

namespace FinancialAdvisorTelegramBot.Bot.Views.Transactions
{
    public class CreateTransactionCommand : ICommand
    {
        private enum CreatingTransactionStatus
        {
            AskType, AskAmount, AskCommunicator, AskCategory, AskTransactionTime, AskDetails, Finished
        }

        private const string IncomeCommand = "/income";
        private const string ExpenseCommand = "/expense";
        private const string TimeNowCommand = "/now";

        public static string TEXT_STYLE => "Create new transaction";
        public static string DEFAULT_STYLE => "/create";
        public bool IsFinished { get; private set; } = false;
        public bool ShowContextMenuAfterExecution => true;

        private CreatingTransactionStatus _status => (CreatingTransactionStatus)Status;
        [CommandPropertySerializable] public int Status { get; set; }
        [CommandPropertySerializable] public bool IsIncomeType { get; set; }
        [CommandPropertySerializable] public decimal Amount { get; set; }
        [CommandPropertySerializable] public string? Communicator { get; set; }
        [CommandPropertySerializable] public int CategoryId { get; set; }
        [CommandPropertySerializable] public DateTime TransactionTime { get; set; }
        [CommandPropertySerializable] public string? Details { get; set; }

        private readonly IBot _bot;
        private readonly IAccountService _accountService;
        private readonly ITransactionService _transactionService;
        private readonly ICategoryService _categoryService;

        public CreateTransactionCommand(IBot bot, IAccountService accountService, ITransactionService transactionService, ICategoryService categoryService)
        {
            _bot = bot;
            _accountService = accountService;
            _transactionService = transactionService;
            _categoryService = categoryService;
        }

        public bool CanExecute(UpdateArgs update, TelegramUser user)
        {
            var splitContextMenu = (string.IsNullOrEmpty(user.ContextMenu) ? string.Empty : user.ContextMenu).Split('/');
            return (splitContextMenu.Length == 3 
                && splitContextMenu[0] == ContextMenus.Accounts 
                && splitContextMenu[2] == ContextMenus.Transaction)
                && (update.GetTextData() == DEFAULT_STYLE
                || update.GetTextData() == TEXT_STYLE)
                && user.UserId is not null;
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
                CreatingTransactionStatus.AskCommunicator => AskCommunicator(user, text, splitContextMenu),
                CreatingTransactionStatus.AskCategory => AskCategory(user, text, splitContextMenu),
                CreatingTransactionStatus.AskTransactionTime => AskTransactionTime(user, text, splitContextMenu),
                CreatingTransactionStatus.AskDetails => AskDetails(user, text, splitContextMenu),
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

        private async Task ProcessResult(TelegramUser user, string text, string[] splitContextMenu)
        {
            Details = text == GeneralCommands.SetEmpty ? null : text;

            if (Communicator is null) throw new InvalidDataException("Communicator cannot be null");

            int accountId = (await _accountService.GetByName(user.UserId 
                ?? throw new InvalidDataException("User id cannot be null"), splitContextMenu[1])
                ?? throw new InvalidDataException("Account not found")).Id;

            Transaction transaction = await _transactionService.Create(IsIncomeType ? Amount : -Amount, 
                Communicator, accountId, CategoryId, TransactionTime, Details);

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Transaction has been created successfully." +
                $"\n({(IsIncomeType ? "Received" : "Sent")} <code>{Math.Abs(transaction.Amount)}</code> " +
                $"{(IsIncomeType ? "from" : "to")} <code>{transaction.Communicator}</code>)"
            });
        }
        
        private async Task AskDetails(TelegramUser user, string text, string[] splitContextMenu)
        {
            TransactionTime = text == TimeNowCommand 
                ? DateTime.Now 
                : Converters.ToDateTime(text);

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Write transaction details:",
                Placeholder = "Write details",
                MarkupType = ReplyMarkupType.InlineKeyboard,
                InlineKeyboardButtons = new() { new() { new("Set details empty", GeneralCommands.SetEmpty) } }
            });
        }

        private async Task AskTransactionTime(TelegramUser user, string text, string[] splitContextMenu)
        {
            string categoryName = text;
            CategoryId = (await _categoryService.GetOrOtherwiseCreateCategory(user.UserId
                ?? throw new InvalidDataException("User id cannot be null"), categoryName)).Id;

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Write transaction time:" +
                $"\n<code>(in next time format '{Converters.DateTimeFormat}')</code>",
                Placeholder = "Write time",
                MarkupType = ReplyMarkupType.InlineKeyboard,
                InlineKeyboardButtons = new() { new() { new("Set transaction time to now", TimeNowCommand) } }
            });
        }

        private async Task AskCategory(TelegramUser user, string text, string[] splitContextMenu)
        {
            string communicator = text.Trim();
            Validators.ValidateName(communicator, isLong: true);
            Communicator = communicator;

            var categories = await _categoryService.GetUserCategories(user.UserId 
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

        private async Task AskCommunicator(TelegramUser user, string text, string[] splitContextMenu)
        {
            Amount = Converters.ToDecimal(text);
            if (Amount <= 0) throw new ArgumentException("Amount must be more than 0");

            await _bot.Write(user, new TextMessageArgs
            {
                Text = IsIncomeType ? "Enter sender:" : "Enter recipient:"
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
                    new() { new("Income", IncomeCommand), new("Expense", ExpenseCommand) }
                }
            });
        }
    }
}