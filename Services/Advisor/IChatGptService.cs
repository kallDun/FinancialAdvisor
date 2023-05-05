using FinancialAdvisorTelegramBot.Services.Advisor.RequestBody;

namespace FinancialAdvisorTelegramBot.Services.Advisor
{
    public interface IChatGptService
    {
        Task<ChatGptReturnBody?> CreateRequest(string prompt);
    }
}
