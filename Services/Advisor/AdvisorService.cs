using FinancialAdvisorTelegramBot.Bot;
using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Advisor.RequestBody;
using FinancialAdvisorTelegramBot.Utils.Attributes;

namespace FinancialAdvisorTelegramBot.Services.Advisor
{
    [CustomService]
    public class AdvisorService : IAdvisorService
    {
        private static readonly HashSet<int> _servicesActive = new();

        private readonly IChatGptService _chatGptService;
        private readonly IBot _bot;

        public AdvisorService(IChatGptService chatGptService, IBot bot)
        {
            _chatGptService = chatGptService;
            _bot = bot;
        }

        public async void WriteSimpleAdviceInBackground(TelegramUser user, User profile)
        {
            await WriteAdviceInBackground(user, async () =>
            {
                string prompt = $"Hi. My name is {profile.FirstName} {profile.LastName}. My occupation is {profile.Occupation}. " +
                $"Write me a few simple financial advices so that I can follow them and always had money. 2-3 advices at least. " +
                $"Create personal advices for my occupation";
                ChatGptReturnBody? response = await _chatGptService.CreateRequest(prompt);
                string? advice = response?.Choices.FirstOrDefault()?.Message.Content;
                if (advice is null) throw new BadHttpRequestException("Bad connection to the openAI server. Try again...");
                return advice;
            });
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
