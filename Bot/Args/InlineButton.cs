namespace FinancialAdvisorTelegramBot.Bot.Args
{
    public class InlineButton
    {
        public InlineButton(string text, string callbackData)
        {
            Text = text;
            CallbackData = callbackData;
        }

        public string Text { get; set; }

        public string CallbackData { get; set; }
    }
}
