using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;
using FinancialAdvisorTelegramBot.Services.Statistics;
using FinancialAdvisorTelegramBot.Services.Statistics.Model;
using FinancialAdvisorTelegramBot.Utils;
using FinancialAdvisorTelegramBot.Utils.CommandSerializing;

namespace FinancialAdvisorTelegramBot.Bot.Views.Statistics
{
    public class GroupBundlesStatisticsCommand : ICommand
    {
        private enum MonthlyStatisticsStatus
        {
            AskGroupsCount, AskBundlesCount, Finished
        }

        public static string TEXT_STYLE => "Group bundles statistics";
        public static string DEFAULT_STYLE => "/bundles";
        public bool IsFinished { get; private set; } = false;

        private MonthlyStatisticsStatus _status => (MonthlyStatisticsStatus)Status;
        [CommandPropertySerializable] public int Status { get; set; }
        [CommandPropertySerializable] public byte GroupsCountInOneBundle { get; set; }
        [CommandPropertySerializable] public byte BundlesCount { get; set; }
        
        private readonly IBot _bot;
        private readonly IUserService _userService;
        private readonly IStatisticsService _statisticsService;

        public GroupBundlesStatisticsCommand(IBot bot, IUserService userService, IStatisticsService statisticsService)
        {
            _bot = bot;
            _userService = userService;
            _statisticsService = statisticsService;
        }

        public bool IsContextMenu(string[] contextMenu)
            => contextMenu.Length == 1
            && contextMenu[0] == ContextMenus.Statistics;

        public bool CanExecute(UpdateArgs update, TelegramUser user)
            => user.ContextMenu == ContextMenus.Statistics
            && (update.GetTextData() == DEFAULT_STYLE
            || update.GetTextData() == TEXT_STYLE)
            && user.UserId is not null;

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            string text = update.GetTextData();
            Task function = _status switch
            {
                MonthlyStatisticsStatus.AskGroupsCount => AskGroupsCount(text, user),
                MonthlyStatisticsStatus.AskBundlesCount => AskStatisticsCount(text, user),
                MonthlyStatisticsStatus.Finished => ProcessResult(text, user),
                _ => throw new InvalidDataException("Invalid status")
            };
            await function;

            if (_status is MonthlyStatisticsStatus.Finished)
            {
                IsFinished = true;
            }
            Status++;
        }

        private async Task ProcessResult(string text, TelegramUser user)
        {
            User profile = await _userService.GetById(user.UserId
                ?? throw new InvalidDataException("Missing profile id"))
                ?? throw new InvalidDataException("Profile not found");

            BundlesCount = Converters.ToByte(text);
            if (BundlesCount <= 0 || BundlesCount > 12) throw new ArgumentException("Cannot create more than 12 bundles and less than 1");

            IList<GroupsStatistic> statistics = await _statisticsService.GetGroupsStatistics(profile, GroupsCountInOneBundle, BundlesCount);

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Statistics for {statistics.Count} group bundles:\n" + 
                string.Join("\n-------------------------------", statistics.Select(x =>
                $"\nDates: <code>{x.DateFrom:dd.MM.yyyy} - {x.DateTo:dd.MM.yyyy}</code>" +
                $"\nTotal income: <code>{x.TotalIncome:0.####}</code>" +
                $"{GetPercentageTextByCategories(x.TotalIncomePerCategories, x.TotalIncome)}" +
                $"\nTotal expense: <code>{x.TotalExpense:0.####}</code>" +
                $"{GetPercentageTextByCategories(x.TotalExpensePerCategories, x.TotalExpense)}")) 
            });
        }

        private string GetPercentageTextByCategories(Dictionary<string, decimal> totalIncomePerCategories, decimal total)
        {
            string text = "";
            foreach (var item in totalIncomePerCategories)
            {
                if (item.Value == 0) continue;
                text += $"\n    <code>•{item.Key} - {(item.Value / total * 100):0.##}%</code>";
            }
            return text == "" ? "" : text;
        }

        private async Task AskStatisticsCount(string text, TelegramUser user)
        {
            User profile = await _userService.GetById(user.UserId
                ?? throw new InvalidDataException("Missing profile id"))
                ?? throw new InvalidDataException("Profile not found");

            GroupsCountInOneBundle = Converters.ToByte(text);
            if (GroupsCountInOneBundle <= 0 || GroupsCountInOneBundle > 20) 
                throw new ArgumentException("Bundle cannot have more than 20 groups and less than 1");

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Bundle has {GroupsCountInOneBundle * profile.DaysInGroup} days"
            });
            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Write total bundles count:"
            });
        }

        private async Task AskGroupsCount(string text, TelegramUser user)
        {
            User profile = await _userService.GetById(user.UserId
                ?? throw new InvalidDataException("Missing profile id"))
                ?? throw new InvalidDataException("Profile not found");

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Write count of groups in one bundle:" +
                $"\n<code>(one group is {profile.DaysInGroup} days, " +
                $"month in average has {Math.Round(30.0 / profile.DaysInGroup)} groups)</code>",
                MarkupType = ReplyMarkupType.InlineKeyboard,
                InlineKeyboardButtons = new List<List<InlineButton>>
                {
                    new() 
                    { 
                        new InlineButton("Month", Math.Round(30.0 / profile.DaysInGroup).ToString()),
                        new InlineButton("Week", Math.Round(7.0 / profile.DaysInGroup).ToString())
                    }
                }
            });
        }
    }
}
