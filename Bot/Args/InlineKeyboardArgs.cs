using Telegram.Bot.Types.Enums;

namespace FinancialAdvisorTelegramBot.Bot.ReplyArgs
{
    public struct InlineKeyboardArgs
    {
        public InlineKeyboardArgs() { }

        public string Text { get; set; } = "";

        public IEnumerable<string> Buttons { get; set; } = new List<string>();

        public string Placeholder { get; set; } = "";

        public ParseMode ParseMode { get; set; } = ParseMode.Html;

        public bool ResizeKeyboard { get; set; } = true;

        public bool OneTimeKeyboard { get; set; } = true;
    }
}
