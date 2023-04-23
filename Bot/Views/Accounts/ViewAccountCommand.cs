using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;
using System.Text.RegularExpressions;

namespace FinancialAdvisorTelegramBot.Bot.Views.Accounts
{
    public class ViewAccountCommand : ICommand
    {
        public static string TEXT_STYLE => "View account";
        public static string DEFAULT_STYLE => "/view";

        private readonly IBot _bot;
        private readonly IAccountService _accountService;

        public ViewAccountCommand(IBot bot, IAccountService accountService)
        {
            _bot = bot;
            _accountService = accountService;
        }

        public bool CanExecute(UpdateArgs update, TelegramUser user) =>
            Regex.IsMatch(user.ContextMenu ?? string.Empty, $"^({ContextMenus.Accounts})[/](.*?)$")
            && (update.GetTextData() == DEFAULT_STYLE || update.GetTextData() == TEXT_STYLE)
            && user.UserId != null;

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            var split = user.ContextMenu?.Split('/') ?? throw new InvalidDataException("Missing context menu");
            if (split.Length < 2) throw new InvalidDataException("Invalid context menu");
            string name = split[1];
            Account account = await _accountService.GetByName(user.UserId
                ?? throw new InvalidDataException("User id cannot be null"), name)
                ?? throw new InvalidDataException($"Cannot find account with name {name}");

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"<u><b>Account:</b></u>" +
                $"\nName: <code>{account.Name}</code>" +
                $"\nDescription: <code>{(string.IsNullOrEmpty(account.Description) ? "none" : account.Description)}</code>" +
                $"\nBalance: <code>{account.CurrentBalance}</code>"
            });
        }
    }
}
