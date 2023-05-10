using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;
using FinancialAdvisorTelegramBot.Utils.CommandSerializing;
using FinancialAdvisorTelegramBot.Utils;
using FinancialAdvisorTelegramBot.Services.Telegram;

namespace FinancialAdvisorTelegramBot.Bot.Views.Accounts
{
    public class UpdateAccountCommand : ICommand
    {
        private enum UpdatingAccountStatus
        {
            AskName, AskDescription, AskCreditLimit, Finished
        }

        public static string TEXT_STYLE => "Update account";
        public static string DEFAULT_STYLE => "/update";
        public bool IsFinished { get; private set; } = false;
        public bool ShowContextMenuAfterExecution => true;

        private readonly IBot _bot;
        private readonly IAccountService _accountService;
        private readonly ITelegramUserService _telegramUserService;
        private readonly IUserService _userService;

        public UpdateAccountCommand(IBot bot, IAccountService accountService, ITelegramUserService telegramUserService, IUserService userService)
        {
            _bot = bot;
            _accountService = accountService;
            _telegramUserService = telegramUserService;
            _userService = userService;
        }

        private UpdatingAccountStatus _status => (UpdatingAccountStatus)Status;
        [CommandPropertySerializable] public int Status { get; set; }
        [CommandPropertySerializable] public bool SkipName { get; set; }
        [CommandPropertySerializable] public string? Name { get; set; }
        [CommandPropertySerializable] public bool SkipDescription { get; set; }
        [CommandPropertySerializable] public string? Description { get; set; }
        [CommandPropertySerializable] public bool SkipCreditLimit { get; set; }
        [CommandPropertySerializable] public decimal CreditLimit { get; set; }


        public bool CanExecute(UpdateArgs update, TelegramUser user) 
        {
            var splitContextMenu = user.ContextMenu?.Split('/') ?? throw new InvalidDataException("Missing context menu");
            return (splitContextMenu.Length == 2 && splitContextMenu[0] == ContextMenus.Account)
                && (update.GetTextData() == DEFAULT_STYLE || update.GetTextData() == TEXT_STYLE)
                && user.UserId != null;
        }

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            string[] splitContextMenu = user.ContextMenu?.Split('/') ?? throw new InvalidDataException("Missing context menu");
            string text = update.GetTextData();
            Task function = _status switch
            {
                UpdatingAccountStatus.AskName => AskName(user),
                UpdatingAccountStatus.AskDescription => AskDescription(user, text),
                UpdatingAccountStatus.AskCreditLimit => AskCreditLimit(user, text),
                UpdatingAccountStatus.Finished => ProcessResult(user, text, splitContextMenu),
                _ => throw new InvalidDataException("Invalid status")
            };
            await function;

            if (_status is UpdatingAccountStatus.Finished)
            {
                IsFinished = true;
            }
            Status++;
        }

        private async Task ProcessResult(TelegramUser user, string text, string[] splitContextMenu)
        {
            if (text != GeneralCommands.Skip)
            {
                CreditLimit = Converters.ToDecimal(text.Trim());
            }
            SkipCreditLimit = text == GeneralCommands.Skip;

            string accountName = splitContextMenu[1];
            Account account = await _accountService.GetByName(user.Id, accountName
                ?? throw new InvalidDataException("Name cannot be empty"))
                ?? throw new ArgumentException($"Account with name {accountName} not found");
            User profile = await _userService.GetById(user.UserId
                ?? throw new ArgumentNullException("Profile id cannot be null"))
                ?? throw new InvalidDataException("Profile cannot be null");

            if (SkipName && SkipDescription && SkipCreditLimit) 
                throw new ArgumentException("You didn't update any field");

            if (!SkipName && accountName == Name) throw new ArgumentException("Name cannot be the same");
            if (!SkipName) account.Name = Name ?? throw new InvalidDataException("Name cannot be null");
            if (!SkipDescription) account.Description = Description;
            if (!SkipCreditLimit) account.CreditLimit = CreditLimit;
            account = await _accountService.Update(profile, account, nameUpdated: !SkipName);

            if (!SkipName)
            {
                await _telegramUserService.SetContextMenu(user, $"{ContextMenus.Account}/{account.Name}");
            }

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Account <code>{account.Name}</code> has been updated"
            });
        }

        private async Task AskCreditLimit(TelegramUser user, string text)
        {
            if (text != GeneralCommands.SetEmpty
                && text != GeneralCommands.Skip)
            {
                Description = text.Trim();
            }
            SkipDescription = text == GeneralCommands.Skip;

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Write your new account credit limit (positive):",
                MarkupType = ReplyMarkupType.InlineKeyboard,
                InlineKeyboardButtons = new()
                {
                    new()
                    {
                        new InlineButton("Do not update credit limit", GeneralCommands.Skip)
                    }
                }
            });
        }

        private async Task AskDescription(TelegramUser user, string text)
        {
            if (text != GeneralCommands.Skip)
            {
                string name = text.Trim();
                Validators.ValidateName(name);
                Name = name;
            }
            SkipName = text == GeneralCommands.Skip;
            
            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Write your new account description:",
                MarkupType = ReplyMarkupType.InlineKeyboard,
                InlineKeyboardButtons = new()
                {
                    new()
                    {
                        new InlineButton("Set description empty", GeneralCommands.SetEmpty)
                    },
                    new()
                    {
                        new InlineButton("Do not update description", GeneralCommands.Skip)
                    }
                }
            });
        }

        private async Task AskName(TelegramUser user)
        {
            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Write your new account name:",
                MarkupType = ReplyMarkupType.InlineKeyboard,
                InlineKeyboardButtons = new()
                {
                    new()
                    {
                        new InlineButton("Do not update name", GeneralCommands.Skip)
                    }
                }
            });
        }
    }
}
