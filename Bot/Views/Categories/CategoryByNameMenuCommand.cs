using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Bot.Views.LimitByCategories;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;
using FinancialAdvisorTelegramBot.Services.Telegram;

namespace FinancialAdvisorTelegramBot.Bot.Views.Categories
{
    public class CategoryByNameMenuCommand : ICommand
    {
        public static string TEXT_STYLE => "Category menu";
        public static string DEFAULT_STYLE => "/category_menu";

        private readonly IBot _bot;
        private readonly ITelegramUserService _telegramUserService;
        private readonly ICategoryService _categoryService;

        public CategoryByNameMenuCommand(IBot bot, ITelegramUserService telegramUserService, ICategoryService categoryService)
        {
            _bot = bot;
            _telegramUserService = telegramUserService;
            _categoryService = categoryService;
        }

        public bool IsContextMenu(string[] contextMenu)
            => contextMenu.Length == 2
            && contextMenu[0] == ContextMenus.Category;

        public bool CanExecute(UpdateArgs update, TelegramUser user)
        {
            var split = (string.IsNullOrEmpty(user.ContextMenu) ? string.Empty : user.ContextMenu).Split('/');
            return split.Length >= 1 && split[0] == ContextMenus.Category
                && (update.GetTextData() == DEFAULT_STYLE || update.GetTextData() == TEXT_STYLE)
                && user.UserId != null;
        }

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            var split = user.ContextMenu?.Split('/') ?? throw new InvalidDataException("Missing context menu");
            string name = split[1];

            List<string> buttons = await _categoryService.GetByName(user.UserId
                ?? throw new InvalidDataException("User id cannot be null"), name) is not null
                ? new()
                {
                    ViewCategoryCommand.TEXT_STYLE,
                    LimitByCategoriesMenuCommand.TEXT_STYLE,
                    CategoriesMenuCommand.TEXT_STYLE
                }
                : new()
                {
                    CategoriesMenuCommand.TEXT_STYLE
                };

            await _telegramUserService.SetContextMenu(user, $"{ContextMenus.Category}/{name}");

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"<b>↓ Category <code>{name}</code> ↓</b>",
                Placeholder = "Select command",
                MarkupType = ReplyMarkupType.ReplyKeyboard,
                ReplyKeyboardButtons = buttons,
            });
        }
    }
}
