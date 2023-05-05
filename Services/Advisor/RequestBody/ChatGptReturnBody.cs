namespace FinancialAdvisorTelegramBot.Services.Advisor.RequestBody
{
    public class ChatGptReturnBody
    {
        public string Id { get; set; }
        
        public string Object { get; set; }
        
        public long Created { get; set; }
        
        public string Model { get; set; }
        
        public ChatGptUsage Usage { get; set; }
        
        public List<ChatGptChoise> Choices { get; set; }
    }

    public class ChatGptUsage
    {
        public int PromptTokens { get; set; }
        
        public int CompletionTokens { get; set; }
        
        public int TotalTokens { get; set; }
    }

    public class ChatGptChoise
    {
        public string FinishReason { get; set; }
        
        public ChatGptMessage Message { get; set; }
        
        public int Index { get; set; }
    }
}
