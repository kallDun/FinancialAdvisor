using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;
using FinancialAdvisorTelegramBot.Services.Telegram;

namespace FinancialAdvisorTelegramBot.Bot.Views.Categories
{
    public class CategoriesMenuCommand : ICommand
    {
        public static string TEXT_STYLE => "Categories";
        public static string DEFAULT_STYLE => "/categories_menu";

        private readonly IBot _bot;
        private readonly ITelegramUserService _telegramUserService;
        private readonly ICategoryService _categoryService;

        public CategoriesMenuCommand(IBot bot, ITelegramUserService telegramUserService, ICategoryService categoryService)
        {
            _bot = bot;
            _telegramUserService = telegramUserService;
            _categoryService = categoryService;
        }

        public bool IsContextMenu(string[] contextMenu)
            => contextMenu.Length == 1
            && contextMenu[0] == ContextMenus.Category;

        public bool CanExecute(UpdateArgs update, TelegramUser user)
            => (update.GetTextData() == DEFAULT_STYLE
            || update.GetTextData() == TEXT_STYLE)
            && user.UserId is not null;

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            List<string> buttons = await _categoryService.HasAny(user.UserId
                ?? throw new InvalidDataException("User id cannot be null"))
                ? new()
                {
                    SelectCategoryCommand.TEXT_STYLE,
                    ViewManyCategoriesCommand.TEXT_STYLE,
                    CreateCategoryCommand.TEXT_STYLE,
                    MainMenuCommand.TEXT_STYLE
                }
                : new()
                {
                    CreateCategoryCommand.TEXT_STYLE,
                    MainMenuCommand.TEXT_STYLE
                };

            await _telegramUserService.SetContextMenu(user, ContextMenus.Category);

            await _bot.Write(user, new TextMessageArgs
            {
                Text = "<b>↓ Categories menu ↓</b>",
                Placeholder = "Select command",
                MarkupType = ReplyMarkupType.ReplyKeyboard,
                ReplyKeyboardButtons = buttons,
            });
        }
    }
}
