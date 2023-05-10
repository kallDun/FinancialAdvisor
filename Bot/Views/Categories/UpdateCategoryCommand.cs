using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;
using FinancialAdvisorTelegramBot.Services.Telegram;
using FinancialAdvisorTelegramBot.Utils;
using FinancialAdvisorTelegramBot.Utils.CommandSerializing;

namespace FinancialAdvisorTelegramBot.Bot.Views.Categories
{
    public class UpdateCategoryCommand : ICommand
    {
        private enum UpdatingCategoryStatus
        {
            AskName, AskDescription, Finished
        }

        public static string TEXT_STYLE => "Update category";
        public static string DEFAULT_STYLE => "/update";
        public bool IsFinished { get; private set; } = false;
        public bool ShowContextMenuAfterExecution => true;

        private readonly IBot _bot;
        private readonly ICategoryService _categoryService;
        private readonly ITelegramUserService _telegramUserService;

        public UpdateCategoryCommand(IBot bot, ICategoryService categoryService, ITelegramUserService telegramUserService)
        {
            _bot = bot;
            _categoryService = categoryService;
            _telegramUserService = telegramUserService;
        }

        private UpdatingCategoryStatus _status => (UpdatingCategoryStatus)Status;
        [CommandPropertySerializable] public int Status { get; set; }
        [CommandPropertySerializable] public string? Name { get; set; }
        [CommandPropertySerializable] public bool SkipName { get; set; }
        [CommandPropertySerializable] public string? Description { get; set; }
        [CommandPropertySerializable] public bool SkipDescription { get; set; }


        public bool CanExecute(UpdateArgs update, TelegramUser user)
        {
            var splitContextMenu = user.ContextMenu?.Split('/') ?? throw new InvalidDataException("Missing context menu");
            return (splitContextMenu.Length == 2 && splitContextMenu[0] == ContextMenus.Category)
                && (update.GetTextData() == DEFAULT_STYLE || update.GetTextData() == TEXT_STYLE)
                && user.UserId != null;
        }

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            string[] splitContextMenu = user.ContextMenu?.Split('/') ?? throw new InvalidDataException("Missing context menu");
            string text = update.GetTextData();
            Task function = _status switch
            {
                UpdatingCategoryStatus.AskName => AskName(user),
                UpdatingCategoryStatus.AskDescription => AskDescription(user, text),
                UpdatingCategoryStatus.Finished => ProcessResult(user, text, splitContextMenu),
                _ => throw new InvalidDataException("Invalid status")
            };
            await function;

            if (_status is UpdatingCategoryStatus.Finished)
            {
                IsFinished = true;
            }
            Status++;
        }

        private async Task ProcessResult(TelegramUser user, string text, string[] splitContextMenu)
        {
            if (text != GeneralCommands.SetEmpty
                && text != GeneralCommands.Skip)
            {
                Description = text.Trim();
            }
            SkipDescription = text == GeneralCommands.Skip;

            string categoryName = splitContextMenu[1];
            if (user.UserId is null) throw new InvalidDataException("User id cannot be null");
            if (Name is null) throw new InvalidDataException("Name cannot be null");
            if (SkipName && SkipDescription) throw new ArgumentException("You didn't update any field");

            Category category = await _categoryService.GetByName(user.UserId.Value, categoryName) 
                ?? throw new InvalidDataException($"Category with name {categoryName} not found");

            if (!SkipName && categoryName == Name) throw new ArgumentException("Name cannot be the same");
            if (!SkipName) category.Name = Name;
            if (!SkipDescription) category.Description = Description;
            category = await _categoryService.Update(user.UserId.Value, category, nameUpdated: !SkipName);
            
            if (!SkipName)
            {
                await _telegramUserService.SetContextMenu(user, $"{ContextMenus.Category}/{category.Name}");
            }

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Category <code>{category.Name}</code> has been updated",
            });
        }

        private async Task AskDescription(TelegramUser user, string text)
        {
            if (text != GeneralCommands.Skip)
            {
                string name = text.Trim();
                Validators.ValidateName(name);
                Name = name;
            }
            SkipName = text == GeneralCommands.Skip;

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Write category description:",
                MarkupType = ReplyMarkupType.InlineKeyboard,
                InlineKeyboardButtons = new List<List<InlineButton>>()
                {
                    new()
                    { 
                        new InlineButton("Set description empty", GeneralCommands.SetEmpty) 
                    },
                    new()
                    {
                        new InlineButton("Do not update description", GeneralCommands.Skip)
                    }
                }
            });
        }

        private async Task AskName(TelegramUser user)
        {
            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Write your new category name:",
                MarkupType = ReplyMarkupType.InlineKeyboard,
                InlineKeyboardButtons = new()
                {
                    new()
                    {
                        new InlineButton("Do not update name", GeneralCommands.Skip)
                    }
                }
            });
        }
    }
}
