using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;

namespace FinancialAdvisorTelegramBot.Bot.Views.Categories
{
    public class ViewManyCategoriesCommand : ICommand
    {
        public static string TEXT_STYLE => "View categories info";
        public static string DEFAULT_STYLE => "/view";

        private readonly IBot _bot;
        private readonly ICategoryService _categoryService;

        public ViewManyCategoriesCommand(IBot bot, ICategoryService categoryService)
        {
            _bot = bot;
            _categoryService = categoryService;
        }

        public bool CanExecute(UpdateArgs update, TelegramUser user)
            => user.ContextMenu == ContextMenus.Category
            && (update.GetTextData() == DEFAULT_STYLE || update.GetTextData() == TEXT_STYLE);

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            var categories = await _categoryService.GetAll(user.UserId
                ?? throw new InvalidDataException("User id cannot be null"));
            if (categories.Count == 0) throw new InvalidDataException("User has no categories");

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"<u><b>Categories:</b></u>\n\n" +
                string.Join("\n", categories.Select((category, index) =>
                    $"{index + 1}. <code>{category.Name}</code>"
                ))
            });
        }
    }
}
