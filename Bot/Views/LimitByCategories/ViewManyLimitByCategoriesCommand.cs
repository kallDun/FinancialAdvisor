using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Operations;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;
using FinancialAdvisorTelegramBot.Services.Operations;

namespace FinancialAdvisorTelegramBot.Bot.Views.LimitByCategories
{
    public class ViewManyLimitByCategoriesCommand : ICommand
    {
        public static string TEXT_STYLE => "View limits short info";
        public static string DEFAULT_STYLE => "/view";

        private readonly IBot _bot;
        private readonly IUserService _userService;
        private readonly ILimitByCategoryService _limitByCategoryService;

        public ViewManyLimitByCategoriesCommand(IBot bot, IUserService userService, ILimitByCategoryService limitByCategoryService)
        {
            _bot = bot;
            _userService = userService;
            _limitByCategoryService = limitByCategoryService;
        }

        public bool CanExecute(UpdateArgs update, TelegramUser user)
        {
            var splitContextMenu = user.ContextMenu?.Split('/') ?? throw new InvalidDataException("Missing context menu");
            return (splitContextMenu.Length == 3 && splitContextMenu[0] == ContextMenus.Category && splitContextMenu[2] == ContextMenus.LimitByCategory)
                && (update.GetTextData() == DEFAULT_STYLE || update.GetTextData() == TEXT_STYLE)
                && user.UserId != null;
        }

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            var splitContextMenu = user.ContextMenu?.Split('/') ?? throw new InvalidDataException("Missing context menu");
            string name = splitContextMenu[1];

            User profile = await _userService.GetById(user.UserId 
                ?? throw new InvalidDataException("Missing profile id"))
                ?? throw new InvalidDataException("Missing profile");

            IList<LimitByCategory> limits = await _limitByCategoryService.GetLimitByCategoriesInfo(profile, name);
            if (limits.Count == 0) throw new InvalidDataException("Limits not found");
            DateTime now = DateTime.Now;
            
            const int totalCharactersCount = 20;
            var limitsPercentage = limits
                .Select(async limit => (
                    Limit: limit, 
                    Expense: await _limitByCategoryService.GetTotalExpenseAmountByLimit(profile, limit, now)))
                .ToDictionary(x => x.Result.Limit, x => (
                    Percent: x.Result.Expense / x.Result.Limit.ExpenseLimit,
                    Characters: (int)Math.Round(((x.Result.Expense > x.Result.Limit.ExpenseLimit ? x.Result.Limit.ExpenseLimit : x.Result.Expense) 
                        / x.Result.Limit.ExpenseLimit) * totalCharactersCount),
                    ExpenseLeft: x.Result.Limit.ExpenseLimit - x.Result.Expense));

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"<u><b>Limits by category {name}:</b></u>\n" +
                string.Join("\n-------------------------------", limits.Select(limit =>
                    $"\nExpense left: <code>{limitsPercentage[limit].ExpenseLeft:0.##}/{limit.ExpenseLimit:0.##}</code>" +
                    $"\nDays left: <code>{_limitByCategoryService.GetDaysLeft(profile, limit, now)}/{limit.GroupCount * profile.DaysInGroup}</code>" +
                    $"{(limit.Account is not null ? $"\nAccount name: <code>{limit.Account.Name}</code>" : "")}" +
                    $"\n|{string.Join("", Enumerable.Range(0, limitsPercentage[limit].Characters).Select(x => "█"))}" +
                    $"{string.Join("", Enumerable.Range(0, totalCharactersCount - limitsPercentage[limit].Characters).Select(x => "   "))}|" +
                    $" <code>{limitsPercentage[limit].Percent * 100:0.##}%</code>"))
            });
        }
    }
}
