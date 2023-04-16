using Telegram.Bot.Types;

namespace FinancialAdvisorTelegramBot.Bot.Args
{
    public struct UpdateArgs
    {
        public long ChatId { get; set; }

        public User From { get; set; }

        public Message? Message { get; set; }

        public CallbackQuery? CallbackQuery { get; set; }

        public UpdateArgs(long chatId, User from, Message? message = null, CallbackQuery? callbackQuery = null)
        {
            ChatId = chatId;
            From = from;
            Message = message;
            CallbackQuery = callbackQuery;
        }

        public UpdateArgs(Update update)
        {
            Message = update.Message;
            CallbackQuery = update.CallbackQuery;
            From = Message?.From ?? CallbackQuery?.From ?? throw new ArgumentNullException(nameof(update));
            ChatId = Message?.Chat.Id ?? CallbackQuery?.Message?.Chat.Id ?? throw new ArgumentNullException(nameof(update));
        }

        public string GetTextData() 
            => Message?.Text ?? CallbackQuery?.Data ?? throw new ArgumentNullException(nameof(Message));
    }
}
