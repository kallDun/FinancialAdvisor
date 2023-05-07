using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Advisor;
using FinancialAdvisorTelegramBot.Services.Core;

namespace FinancialAdvisorTelegramBot.Bot.Views.Advisor
{
    public class GetAdvancedAdviceWithWeeklyStatisticsMenuCommand : ICommand
    {
        public static string TEXT_STYLE => "Get advanced advice using weekly statistics";
        public static string DEFAULT_STYLE => "/advanced_weekly";

        private readonly IBot _bot;
        private readonly IUserService _userService;
        private readonly IAdvisorService _advisorService;

        public GetAdvancedAdviceWithWeeklyStatisticsMenuCommand(IBot bot, IUserService userService, IAdvisorService advisorService)
        {
            _bot = bot;
            _userService = userService;
            _advisorService = advisorService;
        }

        public bool CanExecute(UpdateArgs update, TelegramUser user) 
            => user.ContextMenu == ContextMenus.Advisor
            && (update.GetTextData() == DEFAULT_STYLE
            || update.GetTextData() == TEXT_STYLE)
            && user.UserId is not null;

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            User profile = await _userService.GetById(user.UserId
                ?? throw new InvalidDataException("User id is null"))
                ?? throw new InvalidDataException("User not found");

            _advisorService.WriteAdvancedAdviceUsingWeeklyStatistics(user, profile);

            await _bot.Write(user, new TextMessageArgs
            {
                Text = "<i>Financial advice will be in one minute...</i>"
            });
        }
    }
}
