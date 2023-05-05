namespace FinancialAdvisorTelegramBot.Services.Advisor.RequestBody
{
    public class ChatGptRequestBody
    {
        public string Model { get; set; }
        
        public List<ChatGptMessage> Messages { get; set; }
    }
}
