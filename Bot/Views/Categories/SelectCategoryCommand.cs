using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;
using FinancialAdvisorTelegramBot.Services.Telegram;
using FinancialAdvisorTelegramBot.Utils.CommandSerializing;

namespace FinancialAdvisorTelegramBot.Bot.Views.Categories
{
    public class SelectCategoryCommand : ICommand
    {
        public static string TEXT_STYLE => "Select category";
        public static string DEFAULT_STYLE => "/select";
        public bool IsFinished { get; private set; } = false;
        public bool ShowContextMenuAfterExecution => true;

        [CommandPropertySerializable] public int Status { get; set; }

        private readonly IBot _bot;
        private readonly ITelegramUserService _telegramUserService;
        private readonly ICategoryService _categoryService;

        public SelectCategoryCommand(IBot bot, ITelegramUserService telegramUserService, ICategoryService categoryService)
        {
            _bot = bot;
            _telegramUserService = telegramUserService;
            _categoryService = categoryService;
        }

        public bool CanExecute(UpdateArgs update, TelegramUser user)
            => user.ContextMenu == ContextMenus.Category
            && (update.GetTextData() == DEFAULT_STYLE
            || update.GetTextData() == TEXT_STYLE);

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            if (Status == 0)
            {
                await WriteAvailableCategories(user);
                Status++;
            }
            else
            {
                await SetCategoryContextMenu(update, user);
                IsFinished = true;
            }
        }

        private async Task SetCategoryContextMenu(UpdateArgs update, TelegramUser user)
        {
            string name = update.GetTextData().Trim();
            var account = await _categoryService.GetByName(user.UserId
                ?? throw new InvalidDataException("User id is null"), name);
            if (account is null) throw new ArgumentNullException("Category not found");

            await _telegramUserService.SetContextMenu(user, $"{ContextMenus.Category}/{name}");
        }

        private async Task WriteAvailableCategories(TelegramUser user)
        {
            var categories = await _categoryService.GetAll(user.UserId
                ?? throw new InvalidDataException("User id is null"));

            await _bot.Write(user, new TextMessageArgs
            {
                Text = "<b>↓ Available categories ↓</b>",
                Placeholder = "Select command",
                MarkupType = ReplyMarkupType.InlineKeyboard,
                InlineKeyboardButtons = categories
                .Select(account => new List<InlineButton>() { new InlineButton(
                        account.Name ?? "--Empty name--",
                        account.Name ?? GeneralCommands.Cancel) })
                .ToList()
            });
        }
    }
}
