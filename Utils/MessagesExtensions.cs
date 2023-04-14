namespace FinancialAdvisorTelegramBot.Utils
{
    public static class MessagesExtensions
    {
        private const int MAX_SYMBOLS_IN_ONE_MESSAGE = 4096;

        public static void SplitMessage(string text, ref List<string> messages)
        {
            if (text.Length <= MAX_SYMBOLS_IN_ONE_MESSAGE)
            {
                messages.Add(text);
                return;
            }

            string text_1 = "", text_2 = "";
            var split = text.Split('\n', StringSplitOptions.None);
            bool flag = true;
            for (int i = 0; i < split.Length; i++)
            {
                var item = split[i] + "\n";
                if (flag)
                {
                    string temp = text_1 + item;
                    if (temp.Length < MAX_SYMBOLS_IN_ONE_MESSAGE)
                    {
                        text_1 = temp;
                    }
                    else
                    {
                        text_2 += item;
                        flag = false;
                    }
                }
                else text_2 += item;
            }

            SplitMessage(text_1, ref messages);
            if (text_2.Length > 0)
            {
                SplitMessage(text_2, ref messages);
            }
        }
    }
}
