using FinancialAdvisorTelegramBot.Bot;
using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Advisor.RequestBody;
using FinancialAdvisorTelegramBot.Services.Statistics;
using FinancialAdvisorTelegramBot.Services.Statistics.Model;
using FinancialAdvisorTelegramBot.Utils.Attributes;

namespace FinancialAdvisorTelegramBot.Services.Advisor
{
    [CustomService]
    public class AdvisorService : IAdvisorService
    {
        private static readonly HashSet<int> _servicesActive = new();

        private readonly IChatGptService _chatGptService;
        private readonly IStatisticsService _statisticsService;
        private readonly IBot _bot;

        public AdvisorService(IChatGptService chatGptService, IStatisticsService statisticsService, IBot bot)
        {
            _chatGptService = chatGptService;
            _statisticsService = statisticsService;
            _bot = bot;
        }

        public async void WriteSimpleAdvice(TelegramUser user, User profile)
        {
            await WriteAdviceInBackground(user, async () =>
            {
                string prompt = $"Hi. My name is {profile.FirstName} {profile.LastName}. My occupation is {profile.Occupation}. " +
                $"Write me a few simple personal for my occupation financial advices which I can follow and always had money. 2-3 advices at least.";
                ChatGptReturnBody? response = await _chatGptService.CreateRequest(prompt);
                string? advice = response?.Choices.FirstOrDefault()?.Message.Content;
                if (advice is null) throw new BadHttpRequestException("Bad connection to the openAI server. Try again...");
                return advice;
            });
        }

        public async void WriteAdvancedAdviceUsingMonthlyStatistics(TelegramUser user, User profile) 
            => await WriteAdvancedAdviceUsingStatistics(user, profile, "monthly", (byte)Math.Round(30.0 / profile.DaysInGroup), 4);

        public async void WriteAdvancedAdviceUsingWeeklyStatistics(TelegramUser user, User profile) 
            => await WriteAdvancedAdviceUsingStatistics(user, profile, "weekly", (byte)Math.Round(7.0 / profile.DaysInGroup), 12);

        private async Task WriteAdvancedAdviceUsingStatistics(TelegramUser user, User profile, string statisticsName, byte groupsInBound, byte maxBounds)
        {
            await WriteAdviceInBackground(user, async () =>
            {
                string prompt = $"Hi. My name is {profile.FirstName} {profile.LastName}. My occupation is {profile.Occupation}. " +
                $"Write me a few personal for my occupation financial advices using my {statisticsName} financial statistics:\n";
                
                IList<GroupsStatistic> bundles = await _statisticsService.GetGroupsStatistics(profile, groupsInBound, maxBounds);
                if (bundles.Any(x => x.TotalIncome != 0 || x.TotalExpense != 0) == false) 
                    throw new InvalidDataException("Not enough data to generate advice. You should create new transactions");

                prompt += string.Join("\n", bundles.Select(x =>
                $"In period from {x.DateFrom:dd.MM.yyyy} to {x.DateTo:dd.MM.yyyy} " +
                $"my total income = {x.TotalIncome:0.####}{GetPercentageTextByCategories(x.TotalIncomePerCategories, x.TotalIncome)} " +
                $"and my total expense = {x.TotalExpense:0.####}{GetPercentageTextByCategories(x.TotalExpensePerCategories, x.TotalExpense)}."));

                ChatGptReturnBody? response = await _chatGptService.CreateRequest(prompt);
                string? advice = response?.Choices.FirstOrDefault()?.Message.Content;
                if (advice is null) throw new BadHttpRequestException("Bad connection to the openAI server. Try again...");
                return advice;
            });

            string GetPercentageTextByCategories(Dictionary<string, decimal> totalIncomePerCategories, decimal total)
            {
                List<string> text = new();
                foreach (var item in totalIncomePerCategories)
                {
                    if (item.Value == 0) continue;
                    text.Add($"{item.Key}: {(item.Value / total * 100):0.##}%");
                }
                return text.Any() ? $" (by categories: {string.Join(", ", text)})" : "";
            }
        }
        
        private async Task WriteAdviceInBackground(TelegramUser user, Func<Task<string>> getAdviceAsync)
        {
            string message = "<b>[Advisor Service]:</b>\n";
            try
            {
                if (_servicesActive.Contains(user.Id)) throw new Exception("Cannot run many parallel advices");
                _servicesActive.Add(user.Id);
                message += await getAdviceAsync();
            }
            catch (Exception e)
            {
                message += $"Error occurred while processing advice.\n<b>Error:</b> {e.Message}.";
            }
            _servicesActive.Remove(user.Id);
            await _bot.Write(user, new TextMessageArgs() { Text = message });
        }
    }
}
