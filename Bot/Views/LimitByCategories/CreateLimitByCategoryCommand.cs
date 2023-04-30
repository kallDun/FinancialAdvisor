using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Operations;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;
using FinancialAdvisorTelegramBot.Services.Operations;
using FinancialAdvisorTelegramBot.Utils;
using FinancialAdvisorTelegramBot.Utils.CommandSerializing;

namespace FinancialAdvisorTelegramBot.Bot.Views.LimitByCategories
{
    public class CreateLimitByCategoryCommand : ICommand
    {
        private enum CreateLimitByCategoryStatus
        {
            AskAccount, AskLimit, AskGroupIndexDateFrom, AskGroupCount, Finished
        }

        public static string TEXT_STYLE => "Create new limit";
        public static string DEFAULT_STYLE => "/create";
        public bool IsFinished { get; private set; } = false;
        public bool ShowContextMenuAfterExecution => true;

        private CreateLimitByCategoryStatus _status => (CreateLimitByCategoryStatus)Status;
        [CommandPropertySerializable] public int Status { get; set; }
        [CommandPropertySerializable] public string? AccountName { get; set; }
        [CommandPropertySerializable] public decimal ExpenseLimit { get; set; }
        [CommandPropertySerializable] public DateTime GroupIndexDateFrom { get; set; }
        [CommandPropertySerializable] public byte GroupCount { get; set; }

        private readonly IBot _bot;
        private readonly IAccountService _accountService;
        private readonly IUserService _userService;
        private readonly ILimitByCategoryService _limitByCategoryService;

        public CreateLimitByCategoryCommand(IBot bot, IAccountService accountService, IUserService userService, ILimitByCategoryService limitByCategoryService)
        {
            _bot = bot;
            _accountService = accountService;
            _userService = userService;
            _limitByCategoryService = limitByCategoryService;
        }

        public bool CanExecute(UpdateArgs update, TelegramUser user)
        {
            var split = (string.IsNullOrEmpty(user.ContextMenu) ? string.Empty : user.ContextMenu).Split('/');
            return split.Length == 3 && split[2] == ContextMenus.LimitByCategory
                && (update.GetTextData() == DEFAULT_STYLE
                || update.GetTextData() == TEXT_STYLE);
        }

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            var contextMenuSplit = user.ContextMenu?.Split('/') ?? throw new InvalidDataException("Missing context menu");

            string text = update.GetTextData();
            Task function = _status switch
            {
                CreateLimitByCategoryStatus.AskAccount => AskAccount(user, text),
                CreateLimitByCategoryStatus.AskLimit => AskLimit(user, text),
                CreateLimitByCategoryStatus.AskGroupIndexDateFrom => AskGroupIndexDateFrom(user, text),
                CreateLimitByCategoryStatus.AskGroupCount => AskGroupCount(user, text),
                CreateLimitByCategoryStatus.Finished => ProcessResult(user, text, contextMenuSplit),
                _ => throw new InvalidDataException("Invalid status")
            };
            await function;

            if (_status is CreateLimitByCategoryStatus.Finished)
            {
                IsFinished = true;
            }
            Status++;
        }

        
        private async Task ProcessResult(TelegramUser telegramUser, string text, string[] contextMenuSplit)
        {
            GroupCount = Converters.ToByte(text);

            User user = await _userService.GetById(telegramUser.UserId
                ?? throw new InvalidDataException("Missing profile id"))
                ?? throw new InvalidDataException("Profile not found");
            if (AccountName is null) throw new InvalidDataException("Missing account name");

            LimitByCategory limitByCategory = await _limitByCategoryService.Create(user, AccountName, contextMenuSplit[1], ExpenseLimit, GroupCount, GroupIndexDateFrom);

            await _bot.Write(telegramUser, new TextMessageArgs
            {
                Text = $"Limit <code>{limitByCategory.ExpenseLimit}</code> by category <code>{contextMenuSplit[1]}</code> was successfully created"
            });
        }
        
        private async Task AskGroupCount(TelegramUser telegramUser, string text)
        {
            GroupIndexDateFrom = Converters.ToDate(text);

            User user = await _userService.GetById(telegramUser.UserId 
                ?? throw new InvalidDataException("Missing profile id")) 
                ?? throw new InvalidDataException("Profile not found");

            await _bot.Write(telegramUser, new TextMessageArgs
            {
                Text = $"Write count of groups in period:" +
                $"\n<code>(one group is {user.DaysInGroup} days, " +
                $"month in average has {Math.Round(30.0 / user.DaysInGroup)} groups)</code>",
            });
        }

        private async Task AskGroupIndexDateFrom(TelegramUser user, string text)
        {
            ExpenseLimit = Converters.ToDecimal(text);

            DateTime today = DateTime.Today;
            DateTime firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Enter group index date from:" +
                $"(date automatic converts into first day of week)" +
                $"\n<code>date format '{Converters.DateFormat}'</code>",
                Placeholder = "Select command",
                MarkupType = ReplyMarkupType.InlineKeyboard,
                InlineKeyboardButtons = new List<List<InlineButton>>()
                {
                    new() { new("Today", today.ToString(Converters.DateFormat)) },
                    new() { new("1th of this month", firstDayOfMonth.ToString(Converters.DateFormat)) }
                }
            });
        }

        private async Task AskLimit(TelegramUser user, string text)
        {
            if (text != GeneralCommands.SetEmpty)
            {
                string accountName = text.Trim();
                var account = await _accountService.GetByName(user.UserId
                    ?? throw new InvalidDataException("User id is null"), accountName);
                if (account is null) throw new ArgumentNullException("Account not found");
                AccountName = accountName;
            }

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Enter limit amount:"
            });
        }

        private async Task AskAccount(TelegramUser user, string text)
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
                Text = "<b>↓ Available accounts ↓</b>",
                Placeholder = "Select command",
                MarkupType = ReplyMarkupType.InlineKeyboard,
                InlineKeyboardButtons = inlineButtons
            });
        }
    }
}
