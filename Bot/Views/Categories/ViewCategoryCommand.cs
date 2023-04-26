using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;

namespace FinancialAdvisorTelegramBot.Bot.Views.Categories
{
    public class ViewCategoryCommand : ICommand
    {
        public static string TEXT_STYLE => "View category";
        public static string DEFAULT_STYLE => "/view";

        private readonly IBot _bot;
        private readonly ICategoryService _categoryService;

        public ViewCategoryCommand(IBot bot, ICategoryService categoryService)
        {
            _bot = bot;
            _categoryService = categoryService;
        }

        public bool CanExecute(UpdateArgs update, TelegramUser user)
        {
            var splitContextMenu = user.ContextMenu?.Split('/') ?? throw new InvalidDataException("Missing context menu");
            return (splitContextMenu.Length == 2 && splitContextMenu[0] == ContextMenus.Category)
                && (update.GetTextData() == DEFAULT_STYLE || update.GetTextData() == TEXT_STYLE)
                && user.UserId != null;
        }

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            var splitContextMenu = user.ContextMenu?.Split('/') ?? throw new InvalidDataException("Missing context menu");
            string name = splitContextMenu[1];

            Category category = await _categoryService.GetByName(user.UserId
                ?? throw new InvalidDataException("User id cannot be null"), name)
                ?? throw new InvalidDataException($"Cannot find category with name {name}");

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"<u><b>Category:</b></u>" +
                $"\nName: <code>{category.Name}</code>" +
                $"\nDescription: <code>{(string.IsNullOrEmpty(category.Description) ? "none" : category.Description)}</code>"
            });
        }
    }
}
