using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;

namespace FinancialAdvisorTelegramBot.Bot.Views.Accounts
{
    public class ViewAccountsShortInfoCommand : ICommand
    {
        public static string TEXT_STYLE => "View accounts short info";
        public static string DEFAULT_STYLE => "/view";

        private readonly IBot _bot;
        private readonly IAccountService _accountService;

        public ViewAccountsShortInfoCommand(IBot bot, IAccountService accountService)
        {
            _bot = bot;
            _accountService = accountService;
        }

        public bool CanExecute(UpdateArgs update, TelegramUser user)
            => user.ContextMenu == ContextMenus.Accounts
            && (update.GetTextData() == DEFAULT_STYLE || update.GetTextData() == TEXT_STYLE);

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            IList<Account> accounts = await _accountService.GetByUser(user.UserId 
                ?? throw new InvalidDataException("User id cannot be null"));
            if (accounts.Count == 0) throw new InvalidDataException("User has no accounts");

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"<u><b>Accounts:</b></u>\n" +
                string.Join("\n-------------------------------", accounts.Select(account =>
                    $"\nName: <code>{account.Name}</code>" +
                    $"\nCurrent balance: <code>{account.CurrentBalance}</code>"
                ))
            });
        }
    }
}
