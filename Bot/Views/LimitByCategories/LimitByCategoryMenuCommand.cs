using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Bot.Views.Categories;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;
using FinancialAdvisorTelegramBot.Services.Telegram;

namespace FinancialAdvisorTelegramBot.Bot.Views.LimitByCategories
{
    public class LimitByCategoryMenuCommand : ICommand
    {
        public static string TEXT_STYLE => "Limits";
        public static string DEFAULT_STYLE => "/limits_menu";

        private readonly IBot _bot;
        private readonly ITelegramUserService _telegramUserService;
        private readonly ICategoryService _categoryService;

        public LimitByCategoryMenuCommand(IBot bot, ITelegramUserService telegramUserService, ICategoryService categoryService)
        {
            _bot = bot;
            _telegramUserService = telegramUserService;
            _categoryService = categoryService;
        }

        public bool IsContextMenu(string[] contextMenu)
            => contextMenu.Length == 3
            && contextMenu[0] == ContextMenus.Category
            && contextMenu[2] == ContextMenus.LimitByCategory;

        public bool CanExecute(UpdateArgs update, TelegramUser user)
        {
            var split = (string.IsNullOrEmpty(user.ContextMenu) ? string.Empty : user.ContextMenu).Split('/');
            return split.Length >= 2 && split[0] == ContextMenus.Category
                && (update.GetTextData() == DEFAULT_STYLE
                || update.GetTextData() == TEXT_STYLE);
        }

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            var split = user.ContextMenu?.Split('/') ?? throw new InvalidDataException("Missing context menu");
            string name = split[1];

            List<string> buttons = await _categoryService.GetByName(user.UserId
                ?? throw new InvalidDataException("User id cannot be null"), name) is not null
                ? new()
                {
                    CategoryByNameMenuCommand.TEXT_STYLE
                }
                : new()
                {
                    CategoriesMenuCommand.TEXT_STYLE
                };

            await _telegramUserService.SetContextMenu(user, ContextMenus.Category);

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"<b>↓ Limits by category {name} ↓</b>",
                Placeholder = "Select command",
                MarkupType = ReplyMarkupType.ReplyKeyboard,
                ReplyKeyboardButtons = buttons,
            });
        }
    }
}
