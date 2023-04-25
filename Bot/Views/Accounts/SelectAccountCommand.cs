using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;
using FinancialAdvisorTelegramBot.Services.Telegram;
using FinancialAdvisorTelegramBot.Utils.CommandSerializing;

namespace FinancialAdvisorTelegramBot.Bot.Views.Accounts
{
    public class SelectAccountCommand : ICommand
    {
        public static string TEXT_STYLE => "Select account";
        public static string DEFAULT_STYLE => "/select";
        public bool IsFinished { get; private set; } = false;

        [CommandPropertySerializable] public int Status { get; set; }

        private readonly IBot _bot;
        private readonly ITelegramUserService _telegramUserService;
        private readonly IAccountService _accountService;

        public SelectAccountCommand(IBot bot, ITelegramUserService telegramUserService, IAccountService accountService)
        {
            _bot = bot;
            _telegramUserService = telegramUserService;
            _accountService = accountService;
        }

        public bool ShowContextMenuAfterExecution => true;

        public bool CanExecute(UpdateArgs update, TelegramUser user)
            => user.ContextMenu == ContextMenus.Accounts
            && (update.GetTextData() == DEFAULT_STYLE
            || update.GetTextData() == TEXT_STYLE);

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            if (Status == 0)
            {
                await WriteAvailableAccounts(user);
                Status++;
            }
            else
            {
                await SetAccountContextMenu(update, user);
                IsFinished = true;
            }
        }

        private async Task SetAccountContextMenu(UpdateArgs update, TelegramUser user)
        {
            string name = update.GetTextData().Trim();
            var account = await _accountService.GetByName(user.UserId
                ?? throw new InvalidDataException("User id is null"), name);
            if (account is null) throw new ArgumentNullException("Account not found");

            await _telegramUserService.SetContextMenu(user, $"{ContextMenus.Accounts}/{name}");
        }

        private async Task WriteAvailableAccounts(TelegramUser user)
        {
            var accounts = await _accountService.GetByUser(user.UserId
                ?? throw new InvalidDataException("User id is null"));

            await _bot.Write(user, new TextMessageArgs
            {
                Text = "<b>↓ Available accounts ↓</b>",
                Placeholder = "Select command",
                MarkupType = ReplyMarkupType.InlineKeyboard,
                InlineKeyboardButtons = accounts
                .Select(account => new List<InlineButton>() { new InlineButton(
                        account.Name ?? "--Empty name--",
                        account.Name ?? GeneralCommands.Cancel) })
                .ToList()
            });
        }
    }
}
