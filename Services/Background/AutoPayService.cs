using FinancialAdvisorTelegramBot.Bot;
using FinancialAdvisorTelegramBot.Repositories.Telegram;
using FinancialAdvisorTelegramBot.Utils.Attributes;

namespace FinancialAdvisorTelegramBot.Services.Background
{
    [CustomBackgroundService(seconds: 10)]
    public class AutoPayService : IHostedService
    {
        private readonly IBot _bot;
        private readonly ITelegramUserRepository repository;

        public AutoPayService(IBot bot, ITelegramUserRepository repository)
        {
            _bot = bot;
            this.repository = repository;            
        }
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await AutoPayFunction(cancellationToken);
        }

        public async Task AutoPayFunction(CancellationToken cancellationToken)
        {
            var users = await repository.GetAll();
            foreach (var user in users)
            {
                await _bot.Write(user, new Bot.Args.TextMessageArgs
                {
                    Text = "Hi, I'm background service!"
                });
            }
        }
    }
}
