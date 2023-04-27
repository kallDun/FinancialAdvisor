using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;
using FinancialAdvisorTelegramBot.Utils;
using FinancialAdvisorTelegramBot.Utils.CommandSerializing;

namespace FinancialAdvisorTelegramBot.Bot.Views.Categories
{
    public class CreateCategoryCommand : ICommand
    {
        private enum CreatingCategoryStatus
        {
            AskName, AskDescription, Finished
        }

        public static string TEXT_STYLE => "Create new category";
        public static string DEFAULT_STYLE => "/create";
        public bool IsFinished { get; private set; } = false;
        public bool ShowContextMenuAfterExecution => true;

        private readonly IBot _bot;
        private readonly ICategoryService _categoryService;

        private CreatingCategoryStatus _status => (CreatingCategoryStatus)Status;
        [CommandPropertySerializable] public int Status { get; set; }
        [CommandPropertySerializable] public string? Name { get; set; }
        [CommandPropertySerializable] public string? Description { get; set; }

        public CreateCategoryCommand(IBot bot, ICategoryService categoryService)
        {
            _bot = bot;
            _categoryService = categoryService;
        }

        public bool CanExecute(UpdateArgs update, TelegramUser user)
            => user.ContextMenu == ContextMenus.Category
            && (update.GetTextData() == DEFAULT_STYLE || update.GetTextData() == TEXT_STYLE);

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            string text = update.GetTextData();
            Task function = _status switch
            {
                CreatingCategoryStatus.AskName => AskName(user),
                CreatingCategoryStatus.AskDescription => AskDescription(user, text),
                CreatingCategoryStatus.Finished => ProcessResult(user, text),
                _ => throw new InvalidDataException("Invalid status")
            };
            await function;

            if (_status is CreatingCategoryStatus.Finished)
            {
                IsFinished = true;
            }
            Status++;
        }

        private async Task ProcessResult(TelegramUser user, string text)
        {
            if (text != GeneralCommands.SetEmpty)
            {
                Description = text;
            }
            if (user.UserId is null) throw new InvalidDataException("User id cannot be null");
            if (Name is null) throw new InvalidDataException("Name cannot be null");

            Category category = await _categoryService.CreateCategory(user.UserId.Value, Name, Description);

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Category <code>{category.Name}</code> successfully created",
            });
        }

        private async Task AskDescription(TelegramUser user, string text)
        {
            string name = text.Trim();
            Validators.ValidateName(name);
            Name = name;

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Write category description:",
                MarkupType = ReplyMarkupType.InlineKeyboard,
                InlineKeyboardButtons = new List<List<InlineButton>>()
                {
                    new(){ new("Set description empty", GeneralCommands.SetEmpty) }
                }
            });
        }

        private async Task AskName(TelegramUser user)
        {
            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Write category name:"
            });
        }
    }
}
