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
        private static readonly List<TelegramUser> _servicesActive = new();

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
                string prompt = $"Hi. My name is {profile.FirstName} {profile.LastName}. Write me a few simple financial advices so that " +
                $"I can follow them and always had money. 3-4 advices at least.";
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
                if (_servicesActive.Contains(user)) throw new Exception("Cannot run many parallel advices");
                _servicesActive.Add(user);
                message += await getAdviceAsync();
            }
            catch (Exception e)
            {
                message += $"Error occurred while processing advice.\n<b>Error:</b> {e.Message}.";
            }
            _servicesActive.Remove(user);
            await _bot.Write(user, new TextMessageArgs() { Text = message });
        }
    }
}
