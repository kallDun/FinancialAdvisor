using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Operations;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;
using FinancialAdvisorTelegramBot.Services.Operations;
using FinancialAdvisorTelegramBot.Services.Telegram;
using FinancialAdvisorTelegramBot.Utils;

namespace FinancialAdvisorTelegramBot.Bot.Views.LimitByCategories
{
    public class LimitByCategoryByNameMenuCommand : ICommand
    {
        public static string TEXT_STYLE => "Limit menu";
        public static string DEFAULT_STYLE => "/limit_menu";

        private readonly IBot _bot;
        private readonly ITelegramUserService _telegramUserService;
        private readonly IUserService _userService;
        private readonly ILimitByCategoryService _limitByCategoryService;

        public LimitByCategoryByNameMenuCommand(IBot bot, ITelegramUserService telegramUserService, IUserService userService, ILimitByCategoryService limitByCategoryService)
        {
            _bot = bot;
            _telegramUserService = telegramUserService;
            _userService = userService;
            _limitByCategoryService = limitByCategoryService;
        }

        public bool IsContextMenu(string[] contextMenu)
            => contextMenu.Length == 4 && contextMenu[0] == ContextMenus.Category && contextMenu[2] == ContextMenus.LimitByCategory;

        public bool CanExecute(UpdateArgs update, TelegramUser user)
        {
            var split = (string.IsNullOrEmpty(user.ContextMenu) ? string.Empty : user.ContextMenu).Split('/');
            return split.Length >= 4 && split[0] == ContextMenus.Category && split[2] == ContextMenus.LimitByCategory
                && (update.GetTextData() == DEFAULT_STYLE
                || update.GetTextData() == TEXT_STYLE);
        }

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            var contextMenuSplit = user.ContextMenu?.Split('/') ?? throw new InvalidDataException("Missing context menu");

            string categoryName = contextMenuSplit[1];
            decimal maxExpense = Converters.ToDecimal(contextMenuSplit[3]);
            User profile = await _userService.GetById(user.UserId
                ?? throw new InvalidDataException("User id is null"))
                ?? throw new InvalidDataException("User not found");
            LimitByCategory? limit = await _limitByCategoryService.GetLimitByCategory(profile, categoryName, maxExpense, withData: true);
            
            List<string> buttons = limit is not null
                ? new()
                {
                    ViewLimitByCategoryCommand.TEXT_STYLE,
                    LimitByCategoriesMenuCommand.TEXT_STYLE
                }
                : new()
                {
                    LimitByCategoriesMenuCommand.TEXT_STYLE
                };

            await _telegramUserService.SetContextMenu(user, $"{ContextMenus.Category}/{categoryName}/{ContextMenus.LimitByCategory}/{maxExpense}");

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"<b>↓ Limit <code>{maxExpense:0.##}</code> in category {categoryName} ↓</b>",
                Placeholder = "Select command",
                MarkupType = ReplyMarkupType.ReplyKeyboard,
                ReplyKeyboardButtons = buttons,
            });
        }
    }
}
