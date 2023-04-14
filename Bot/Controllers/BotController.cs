using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.ReplyArgs;
using FinancialAdvisorTelegramBot.Bot.Updates;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace FinancialAdvisorTelegramBot.Bot.Controllers
{
    [ApiController]
    [Route("/")]
    public class BotController
    {
        private readonly IBot _bot;
        private readonly ITelegramUpdateDistributor _updateDistributor;

        public BotController(IBot bot, ITelegramUpdateDistributor updateDistributor)
        {
            _bot = bot;
            _updateDistributor = updateDistributor;
        }

        [HttpPost]
        public async Task Post(Update update)
        {
            UpdateArgs updateArgs = new(update);
            try
            {
                await _updateDistributor.SignIn(updateArgs);
                await _updateDistributor.GetUpdate(updateArgs);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Something went wrong in user {updateArgs.From.Username}: " + e);
                await _bot.WriteByChatId(updateArgs.ChatId, new TextMessageArgs
                {
                    Text = $"Something went wrong: {e.Message}"
                });
            }
        }

        [HttpGet]
        public string Get()
        {
            return "Telegram bot was started";
        }
    }
}
