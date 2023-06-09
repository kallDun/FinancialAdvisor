﻿using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Core;
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
            AskAccountName, AskCategory, AskName, AskAmount, AskPayday, AskAutoPay, Finished
        }

        public static string TEXT_STYLE => "Create new subscription";
        public static string DEFAULT_STYLE => "/create";
        public bool IsFinished { get; private set; } = false;
        public bool ShowContextMenuAfterExecution => true;

        private readonly IBot _bot;
        private readonly IAccountService _accountService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly ICategoryService _categoryService;

        private CreatingSubscriptionStatus _status => (CreatingSubscriptionStatus)Status;
        [CommandPropertySerializable] public int Status { get; set; }
        [CommandPropertySerializable] public string? AccountName { get; set; }
        [CommandPropertySerializable] public string? CategoryName { get; set; }
        [CommandPropertySerializable] public string? Name { get; set; }
        [CommandPropertySerializable] public decimal Amount { get; set; }
        [CommandPropertySerializable] public byte Payoutday { get; set; }
        [CommandPropertySerializable] public bool AutoPay { get; set; }

        public CreateSubscriptionCommand(IBot bot, IAccountService accountService, ISubscriptionService subscriptionService, ICategoryService categoryService)
        {
            _bot = bot;
            _accountService = accountService;
            _subscriptionService = subscriptionService;
            _categoryService = categoryService;
        }

        public bool CanExecute(UpdateArgs update, TelegramUser user)
        {
            var contextMenuSplit = (string.IsNullOrEmpty(user.ContextMenu) ? string.Empty : user.ContextMenu).Split('/');
            return ((contextMenuSplit.Length == 3 && contextMenuSplit[2] == ContextMenus.Subscription)
                || contextMenuSplit.Length == 1 && contextMenuSplit[0] == ContextMenus.Subscription)
                && (update.GetTextData() == DEFAULT_STYLE
                || update.GetTextData() == TEXT_STYLE);
        }

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            if (user.UserId is null) throw new InvalidDataException("Profile id cannot be null");
            var contextMenuSplit = user.ContextMenu?.Split('/') ?? throw new InvalidDataException("Missing context menu");
            string text = update.GetTextData();
            
            Task function = _status switch
            {
                CreatingSubscriptionStatus.AskAccountName => AskAccountName(user, text, contextMenuSplit),
                CreatingSubscriptionStatus.AskCategory => AskCategory(user, text),
                CreatingSubscriptionStatus.AskName => AskSubscriptionName(user, text),
                CreatingSubscriptionStatus.AskAmount => AskAmount(user, text),
                CreatingSubscriptionStatus.AskPayday => AskPayday(user, text),
                CreatingSubscriptionStatus.AskAutoPay => AskAutoPayFunction(user, text, contextMenuSplit),
                CreatingSubscriptionStatus.Finished => ProcessResult(user, text, contextMenuSplit),
                _ => throw new InvalidDataException("Invalid status")
            };
            await function;

            if (_status is CreatingSubscriptionStatus.Finished)
            {
                IsFinished = true;
            }
            Status++;
        }

        private async Task ProcessResult(TelegramUser user, string text, string[] contextMenuSplit)
        {
            if (contextMenuSplit.Length == 3)
            {
                AutoPay = text == GeneralCommands.Yes ? true
                    : text == GeneralCommands.No ? false
                    : throw new ArgumentException("You should answered with '/yes' or '/no'");
            }
            else AutoPay = false;

            if (user.UserId is null) throw new InvalidDataException("Profile id cannot be null");
            if (await _accountService.HasAny(user.UserId.Value) == false) throw new InvalidDataException("User has no accounts");
            int? accountId = string.IsNullOrEmpty(AccountName) ? null
                : (await _accountService.GetByName((int)user.UserId, AccountName)
                ?? throw new InvalidDataException($"Cannot find account with name {AccountName}"))
                .Id;
            int categoryId = (await _categoryService.GetByName(user.UserId.Value, CategoryName 
                ?? throw new InvalidDataException("Category name cannot be null")) 
                ?? throw new InvalidDataException($"Cannot find category with name {CategoryName}")).Id;
            if (Name is null) throw new InvalidDataException("Name cannot be null");

            var subscription = await _subscriptionService.Create(user.UserId.Value, accountId, categoryId, Name, Amount, Payoutday, AutoPay);
            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"{Name} subscription has been created.\n" +
                $"<code>Next payment day is {subscription.NextPaymentDate:dd.MM.yyyy}</code>"
                + $" with {subscription.Amount}</code>"
            });
        }

        private async Task AskAutoPayFunction(TelegramUser user, string text, string[] contextMenuSplit)
        {
            Payoutday = Converters.ToByte(text.Trim());

            if (AccountName is null)
            {
                await ProcessResult(user, text, contextMenuSplit);
                return;
            }

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Do you want to auto pay this subscription?",
                Placeholder = "Select command",
                MarkupType = ReplyMarkupType.InlineKeyboard,
                InlineKeyboardButtons = new List<List<InlineButton>>()
                {
                    new() { new("Yes", GeneralCommands.Yes), new("No", GeneralCommands.No) }
                }
            });
        }

        private async Task AskPayday(TelegramUser user, string text)
        {
            Amount = Converters.ToDecimal(text.Trim());
            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Write payout day <i>(from 1 to 31)</i>:"
            });
        }

        private async Task AskAmount(TelegramUser user, string text)
        {
            string name = text.Trim();
            Validators.ValidateName(name);
            Name = name;

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Write positive <i>(income)</i> or negative <i>(expense)</i> amount per month:"
            });
        }

        private async Task AskSubscriptionName(TelegramUser user, string text)
        {
            if (user.UserId is null) throw new InvalidDataException("User id cannot be null");
            string categoryName = text.Trim();
            if (await _categoryService.GetByName(user.UserId.Value, categoryName) is null)
                throw new InvalidDataException($"Cannot find category with name {categoryName}");
            CategoryName = categoryName;

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Write subscription name:"
            });
        }

        private async Task AskCategory(TelegramUser user, string text)
        {
            if (user.UserId is null) throw new InvalidDataException("User id cannot be null");

            if (string.IsNullOrEmpty(AccountName) && text != GeneralCommands.SetEmpty)
            {
                string accountName = text.Trim();
                var account = await _accountService.GetByName(user.UserId.Value, accountName);
                if (account is null) throw new ArgumentNullException("Account not found");
                AccountName = accountName;
            }

            IList<Category> categories = await _categoryService.GetAll(user.UserId.Value);
            if (categories.Any(x => x.Name == CategoryNames.Subscription) == false)
            {
                var subscriptionCategory = await _categoryService.GetOrOtherwiseCreateCategory(user.UserId.Value, CategoryNames.Subscription);
                categories.Add(subscriptionCategory);
            }

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Select category for subscription:",
                Placeholder = "Select category",
                MarkupType = ReplyMarkupType.InlineKeyboard,
                InlineKeyboardButtons = categories.Select(x => new List<InlineButton>()
                {
                    new(x.Name ?? "-- Missing name --",
                        x.Name ?? GeneralCommands.Cancel)
                }).ToList()
            });
        }

        private async Task AskAccountName(TelegramUser user, string text, string[] contextMenuSplit)
        {
            if (contextMenuSplit.Length == 3)
            {
                AccountName = contextMenuSplit[1];
                await _bot.Write(user, new TextMessageArgs
                {
                    Text = $"Subscription will be created in account <code>{AccountName}</code>"
                });
                Status++;
                await AskCategory(user, text);
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
                Text = "<b>↓ Available accounts ↓</b>",
                Placeholder = "Select command",
                MarkupType = ReplyMarkupType.InlineKeyboard,
                InlineKeyboardButtons = inlineButtons
            });
        }
    }
}
