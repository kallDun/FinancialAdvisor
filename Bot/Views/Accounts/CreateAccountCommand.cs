using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;
using FinancialAdvisorTelegramBot.Utils;
using FinancialAdvisorTelegramBot.Utils.CommandSerializing;

namespace FinancialAdvisorTelegramBot.Bot.Views.Accounts
{
    public class CreateAccountCommand : ICommand
    {
        private enum CreatingAccountStatus
        {
            AskName, AskDescription, AskCurrentBalance, Finished
        }

        public static string TEXT_STYLE => "Create new account";
        public static string DEFAULT_STYLE => "/create";
        public bool IsFinished { get; private set; } = false;
        public bool ShowContextMenuAfterExecution => true;

        private readonly IBot _bot;
        private readonly IAccountService _accountService;
        private readonly IUserService _userService;

        private CreatingAccountStatus _status => (CreatingAccountStatus)Status;
        [CommandPropertySerializable] public int Status { get; set; }
        [CommandPropertySerializable] public string? Name { get; set; }
        [CommandPropertySerializable] public string? Description { get; set; }
        [CommandPropertySerializable] public decimal StartBalance { get; set; }

        public CreateAccountCommand(IBot bot, IAccountService accountService, IUserService userService)
        {
            _bot = bot;
            _accountService = accountService;
            _userService = userService;
        }

        public bool CanExecute(UpdateArgs update, TelegramUser user)
            => user.ContextMenu == ContextMenus.Accounts
            && (update.GetTextData() == DEFAULT_STYLE || update.GetTextData() == TEXT_STYLE);
        
        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            string text = update.GetTextData();
            Task function = _status switch
            {
                CreatingAccountStatus.AskName => AskName(user),
                CreatingAccountStatus.AskDescription => AskDescription(user, text),
                CreatingAccountStatus.AskCurrentBalance => AskCurrentBalance(user, text),
                CreatingAccountStatus.Finished => ProcessResult(user, text),
                _ => throw new InvalidDataException("Invalid status")
            };
            await function;

            if (_status is CreatingAccountStatus.Finished)
            {
                IsFinished = true;
            }
            Status++;
        }

        private async Task ProcessResult(TelegramUser user, string text)
        {
            StartBalance = text != GeneralCommands.SetEmpty
                ? Converters.ToDecimal(text.Trim()) : 0;

            if (Name is null) throw new ArgumentNullException("Name cannot be empty");

            User profile = await _userService.GetById(user.UserId
                ?? throw new ArgumentNullException("Profile id cannot be null"))
                ?? throw new InvalidDataException("Profile cannot be null");
            Account account = await _accountService.Create(profile, Name, Description, StartBalance);

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"{account.Name} account has been created"
            });
        }

        private async Task AskCurrentBalance(TelegramUser user, string text)
        {
            if (text != GeneralCommands.SetEmpty)
            {
                string description = text.Trim();
                Description = description;
            }
            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Write account start balance:",
                MarkupType = ReplyMarkupType.InlineKeyboard,
                InlineKeyboardButtons = new()
                {
                    new()
                    {
                        new InlineButton("Set start balance to 0", GeneralCommands.SetEmpty)
                    }
                }
            });
        }

        private async Task AskDescription(TelegramUser user, string text)
        {
            string name = text.Trim();
            Validators.ValidateName(name);
            Name = name;
            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Write account description:",
                MarkupType = ReplyMarkupType.InlineKeyboard,
                InlineKeyboardButtons = new()
                {
                    new()
                    {
                        new InlineButton("Set description empty", GeneralCommands.SetEmpty)
                    }
                }
            });
        }

        private async Task AskName(TelegramUser user)
        {
            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Write account name:"
            });
        }
    }
}
