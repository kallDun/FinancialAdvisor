using Telegram.Bot.Types.Enums;

namespace FinancialAdvisorTelegramBot.Bot.Args
{
    public struct TextMessageArgs
    {
        public TextMessageArgs() { }

        public string Text { get; set; } = "";

        public ParseMode ParseMode { get; set; } = ParseMode.Html;

        public bool DisableWebPagePreview { get; set; } = true;

        public ReplyMarkupType MarkupType { get; set; } = ReplyMarkupType.None;

        /// <summary>
        /// Works only with MarkupType = ReplyMarkupType.ReplyKeyboard
        /// </summary>
        public List<string> ReplyKeyboardButtons { get; set; } = new();

        /// <summary>
        /// Works only with MarkupType = ReplyMarkupType.InlineKeyboard
        /// </summary>
        public List<List<InlineButton>> InlineKeyboardButtons { get; set; } = new();

        /// <summary>
        /// Works only with MarkupType = ReplyMarkupType.ReplyKeyboard or ReplyMarkupType.ForceReply
        /// </summary>
        public string? Placeholder { get; set; }
    }
}
