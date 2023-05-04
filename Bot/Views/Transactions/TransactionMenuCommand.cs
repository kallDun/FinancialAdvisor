using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Bot.Views.Accounts;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;
using FinancialAdvisorTelegramBot.Services.Telegram;

namespace FinancialAdvisorTelegramBot.Bot.Views.Transactions
{
    public class TransactionMenuCommand : ICommand
    {
        public static string TEXT_STYLE => "Transaction menu";
        public static string DEFAULT_STYLE => "/transaction_menu";

        private readonly IBot _bot;
        private readonly ITelegramUserService _telegramUserService;
        private readonly IAccountService _accountService;

        public TransactionMenuCommand(IBot bot, ITelegramUserService telegramUserService, IAccountService accountService)
        {
            _bot = bot;
            _telegramUserService = telegramUserService;
            _accountService = accountService;
        }

        public bool IsContextMenu(string[] contextMenu)
            => (contextMenu.Length == 3 && contextMenu[0] == ContextMenus.Account && contextMenu[2] == ContextMenus.Transaction);

        public bool CanExecute(UpdateArgs update, TelegramUser user)
        {
            var splitContextMenu = (string.IsNullOrEmpty(user.ContextMenu) ? string.Empty : user.ContextMenu).Split('/');
            return splitContextMenu.Length >= 2 && splitContextMenu[0] == ContextMenus.Account
                && (update.GetTextData() == DEFAULT_STYLE
                || update.GetTextData() == TEXT_STYLE)
                && user.UserId is not null;
        }

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            if (user.UserId is null) throw new InvalidDataException("User id cannot be null");
            var splitContextMenu = user.ContextMenu?.Split('/') ?? throw new InvalidDataException("Missing context menu");

            string accountName = splitContextMenu[1];
            var account = await _accountService.GetByName(user.UserId.Value, accountName);

            List<string> buttons = account is not null
                ? new()
                {
                    CreateTransactionCommand.TEXT_STYLE,
                    CreateTransferToAccountCommand.TEXT_STYLE,
                    AccountByNameMenuCommand.TEXT_STYLE
                }
                : new()
                {
                    AccountsMenuCommand.TEXT_STYLE
                };

            await _telegramUserService.SetContextMenu(user, 
                $"{ContextMenus.Account}/{splitContextMenu[1]}/{ContextMenus.Transaction}");
            
            await _bot.Write(user, new TextMessageArgs
            {
                Text = "<b>↓ Transaction menu ↓</b>",
                Placeholder = "Select command",
                MarkupType = ReplyMarkupType.ReplyKeyboard,
                ReplyKeyboardButtons = buttons,
            });
        }
    }
}
