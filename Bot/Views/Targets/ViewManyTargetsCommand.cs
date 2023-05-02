using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Operations;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Operations;
using FinancialAdvisorTelegramBot.Utils.Bot;

namespace FinancialAdvisorTelegramBot.Bot.Views.Targets
{
    public class ViewManyTargetsCommand : ICommand
    {
        public static string TEXT_STYLE => "View targets short info";
        public static string DEFAULT_STYLE => "/view";

        private readonly IBot _bot;
        private readonly ITargetService _targetService;

        public ViewManyTargetsCommand(IBot bot, ITargetService targetService)
        {
            _bot = bot;
            _targetService = targetService;
        }

        public bool CanExecute(UpdateArgs update, TelegramUser user)
        {
            var split = (string.IsNullOrEmpty(user.ContextMenu) ? string.Empty : user.ContextMenu).Split('/');
            return split.Length == 3 && split[0] == ContextMenus.Account && split[2] == ContextMenus.Target
                && (update.GetTextData() == DEFAULT_STYLE
                || update.GetTextData() == TEXT_STYLE);
        }

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            if (user.UserId is null) throw new InvalidDataException("User id cannot be null");
            var contextMenuSplit = user.ContextMenu?.Split('/') ?? throw new InvalidDataException("Missing context menu");

            IList<TargetSubAccount> targets = await _targetService.GetAll(user.UserId.Value, contextMenuSplit[1]);
            var targetsPercentage = targets.ToDictionary(
                x => x, x => (Characters: (int)Math.Round((x.CurrentBalance / x.GoalAmount) * BotWriteUtils.MaxPercentageLength),
                Percent: (x.CurrentBalance / x.GoalAmount) * 100));

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"<u><b>Targets:</b></u>\n" +
                string.Join("\n-------------------------------", targets.Select(target =>
                    $"\nName: <code>{target.Name}</code>" +
                    $"\nAmount collected: <code>{target.CurrentBalance}/{target.GoalAmount}</code>" +
                    $"\n|{BotWriteUtils.GetPercentageString(targetsPercentage[target].Characters)}|" +
                    $" <code>{targetsPercentage[target].Percent:0.##}%</code>"))
            });
        }
    }
}
