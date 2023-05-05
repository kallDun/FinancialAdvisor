using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Advisor;

namespace FinancialAdvisorTelegramBot.Bot.Views.Advisor
{
    public class GetSimpleAdviceMenuCommand : ICommand
    {
        public static string TEXT_STYLE => "Get simple advice";
        public static string DEFAULT_STYLE => "/simple";

        private readonly IBot _bot;
        private readonly IAdvisorService _advisorService;

        public GetSimpleAdviceMenuCommand(IBot bot, IAdvisorService advisorService)
        {
            _bot = bot;
            _advisorService = advisorService;
        }

        public bool CanExecute(UpdateArgs update, TelegramUser user) 
            => user.ContextMenu == ContextMenus.Advisor
            && (update.GetTextData() == DEFAULT_STYLE
            || update.GetTextData() == TEXT_STYLE)
            && user.UserId is not null;

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            _advisorService.WriteSimpleAdviceInBackground(user);

            await _bot.Write(user, new TextMessageArgs
            {
                Text = "<i>Financial advice will be in one minute...</i>"
            });
        }
    }
}
