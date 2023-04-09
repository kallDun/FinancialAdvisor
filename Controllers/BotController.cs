using FinancialAdvisorTelegramBot.Bot;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FinancialAdvisorTelegramBot.Controllers
{
    [ApiController]
    [Route("/")]
    public class BotController
    {
        private readonly IBot _bot;

        public BotController(IBot bot)
        {
            _bot = bot;
        }


        [HttpPost]
        public async void Post(Update update)
        {
            Console.WriteLine(update.Message?.Text);
            await _bot.BotClient.SendTextMessageAsync(update.Message.Chat.Id, "Hi!");
        }
        
        [HttpGet]
        public string Get()
        {
            return "Telegram bot was started";
        }
    }
}
