using Telegram.Bot.Types.Enums;

namespace FinancialAdvisorTelegramBot.Bot.ReplyArgs
{
    public struct TextMessageArgs
    {
        public TextMessageArgs() { }

        public string Text { get; set; } = "";

        public bool HideKeyboard { get; set; } = false;

        public ParseMode ParseMode { get; set; } = ParseMode.Html;

        public bool DisableWebPagePreview { get; set; } = true;
    }
}
