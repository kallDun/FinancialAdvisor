namespace FinancialAdvisorTelegramBot.Utils.Bot
{
    public static class BotWriteUtils
    {
        public static int MaxPercentageLength => 20;

        public static string GetPercentageString(int characters)
        {
            return 
                $"{string.Join("", Enumerable.Range(0, characters).Select(x => "█"))}" +
                $"{string.Join("", Enumerable.Range(0, MaxPercentageLength - characters).Select(x => "   "))}";
        }
    }
}
