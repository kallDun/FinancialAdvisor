using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Operations;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;
using FinancialAdvisorTelegramBot.Services.Operations;
using FinancialAdvisorTelegramBot.Utils;
using FinancialAdvisorTelegramBot.Utils.Bot;

namespace FinancialAdvisorTelegramBot.Bot.Views.LimitByCategories
{
    public class ViewLimitByCategoryCommand : ICommand
    {
        public static string TEXT_STYLE => "View limit";
        public static string DEFAULT_STYLE => "/view";

        private readonly IBot _bot;
        private readonly IUserService _userService;
        private readonly ILimitByCategoryService _limitByCategoryService;

        public ViewLimitByCategoryCommand(IBot bot, IUserService userService, ILimitByCategoryService limitByCategoryService)
        {
            _bot = bot;
            _userService = userService;
            _limitByCategoryService = limitByCategoryService;
        }

        public bool CanExecute(UpdateArgs update, TelegramUser user)
        {
            var split = (string.IsNullOrEmpty(user.ContextMenu) ? string.Empty : user.ContextMenu).Split('/');
            return split.Length == 4 && split[0] == ContextMenus.Category && split[2] == ContextMenus.LimitByCategory
                && (update.GetTextData() == DEFAULT_STYLE
                || update.GetTextData() == TEXT_STYLE);
        }

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            var contextMenuSplit = user.ContextMenu?.Split('/') ?? throw new InvalidDataException("Missing context menu");

            string categoryName = contextMenuSplit[1];
            decimal maxExpense = Converters.ToDecimal(contextMenuSplit[3]);
            User profile = await _userService.GetById(user.UserId 
                ?? throw new InvalidDataException("User id is null")) 
                ?? throw new InvalidDataException("User not found");
            LimitByCategory limit = await _limitByCategoryService.GetLimitByCategory(profile, categoryName, maxExpense, withData: true) 
                ?? throw new InvalidDataException($"Limit {maxExpense} not found");

            DateTime now = DateTime.Now;
            decimal expense = await _limitByCategoryService.GetTotalExpenseAmountByLimit(profile, limit, now);
            decimal percent = expense / limit.ExpenseLimit;
            int characters = (int)Math.Round(((expense > limit.ExpenseLimit ? limit.ExpenseLimit : expense) / limit.ExpenseLimit) * BotWriteUtils.MaxPercentageLength);
            decimal expenseLeft = limit.ExpenseLimit - expense;

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"<u><b>Limit {limit.ExpenseLimit:0.##} by category {categoryName}:</b></u>\n" +
                $"\nExpense left: <code>{expenseLeft:0.##}/{limit.ExpenseLimit:0.##}</code>" +
                $"\nDays left: <code>{_limitByCategoryService.GetDaysLeft(profile, limit, now)}/{limit.GroupCount * profile.DaysInGroup}</code>" +
                $"{(limit.Account is not null ? $"\nAccount name: <code>{limit.Account.Name}</code>" : "")}" +
                $"\n|{BotWriteUtils.GetPercentageString(characters)}|" +
                $" <code>{percent * 100:0.##}%</code>"
            });

        }
    }
}
