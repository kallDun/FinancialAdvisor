using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace FinancialAdvisorTelegramBot.Bot
{
    public class Bot : IBot
    {
        public TelegramBotClient BotClient { get; private set; }

        public Bot(IOptions<BotSettings> options)
        {
            BotClient = new TelegramBotClient(options.Value.Token 
                ?? throw new ArgumentNullException(nameof(options.Value.Token)));
        }
    }
}
