using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Operations;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Operations;
using FinancialAdvisorTelegramBot.Utils.Bot;

namespace FinancialAdvisorTelegramBot.Bot.Views.Targets
{
    public class ViewTargetCommand : ICommand
    {
        public static string TEXT_STYLE => "View target";
        public static string DEFAULT_STYLE => "/view";

        private readonly IBot _bot;
        private readonly ITargetService _targetService;

        public ViewTargetCommand(IBot bot, ITargetService targetService)
        {
            _bot = bot;
            _targetService = targetService;
        }

        public bool CanExecute(UpdateArgs update, TelegramUser user)
        {
            var split = (string.IsNullOrEmpty(user.ContextMenu) ? string.Empty : user.ContextMenu).Split('/');
            return split.Length == 4 && split[0] == ContextMenus.Account && split[2] == ContextMenus.Target
                && (update.GetTextData() == DEFAULT_STYLE
                || update.GetTextData() == TEXT_STYLE);
        }

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            var splitContextMenu = user.ContextMenu?.Split('/') ?? throw new InvalidDataException("Missing context menu");
            string accountName = splitContextMenu[1];
            string targetName = splitContextMenu[3];

            TargetSubAccount target = await _targetService.GetByName(user.UserId
                ?? throw new InvalidDataException("User id cannot be null"), accountName, targetName)
                ?? throw new InvalidDataException("Target not found");

            int characters = (int)Math.Round((target.CurrentBalance / target.GoalAmount) * BotWriteUtils.MaxPercentageLength);
            decimal percent = (target.CurrentBalance / target.GoalAmount) * 100;

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"<u><b>Target:</b></u>" +
                $"\nName: <code>{target.Name}</code>" +
                $"\nAmount collected: <code>{target.CurrentBalance}/{target.GoalAmount}</code>" +
                $"\n|{BotWriteUtils.GetPercentageString(characters)}|" +
                $" <code>{percent:0.##}%</code>" +
                $"\nDescription: <code>{(string.IsNullOrEmpty(target.Description) ? "none" : target.Description)}</code>"
            });
        }
    }
}
