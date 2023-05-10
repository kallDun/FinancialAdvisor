﻿using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;
using FinancialAdvisorTelegramBot.Services.Telegram;
using FinancialAdvisorTelegramBot.Utils.CommandSerializing;

namespace FinancialAdvisorTelegramBot.Bot.Views.Categories
{
    public class DeleteCategoryCommand : ICommand
    {
        public static string TEXT_STYLE => "Delete category";
        public static string DEFAULT_STYLE => "/delete";
        public virtual bool IsFinished { get; private set; } = false;
        public bool ShowContextMenuAfterExecution => true;

        [CommandPropertySerializable] public int Status { get; set; }

        private readonly IBot _bot;
        private readonly ITelegramUserService _telegramUserService;
        private readonly ICategoryService _categoryService;

        public DeleteCategoryCommand(IBot bot, ITelegramUserService telegramUserService, ICategoryService categoryService)
        {
            _bot = bot;
            _telegramUserService = telegramUserService;
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
            string[] splitContextMenu = user.ContextMenu?.Split('/') ?? throw new InvalidDataException("Missing context menu");
            string text = update.GetTextData();

            if (Status == 0)
            {
                await AskForDelete(user, text, splitContextMenu);
                Status++;
            }
            else
            {
                await ProcessDeletion(user, text, splitContextMenu);
                IsFinished = true;
            }
        }

        private async Task ProcessDeletion(TelegramUser user, string text, string[] splitContextMenu)
        {
            if (text == GeneralCommands.Confirm)
            {
                string categoryName = splitContextMenu[1];
                if (user.UserId is null) throw new InvalidOperationException("User id is null");
                await _categoryService.DeleteByName(user.UserId.Value, categoryName);

                await _bot.Write(user, new TextMessageArgs
                {
                    Text = $"Category <code>{categoryName}</code> has been deleted"
                });

                await _telegramUserService.SetContextMenu(user, ContextMenus.Category);
            }
        }

        private async Task AskForDelete(TelegramUser user, string text, string[] splitContextMenu)
        {
            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Are you sure you want to delete category <code>{splitContextMenu[1]}</code> with ALL data?",
                MarkupType = ReplyMarkupType.InlineKeyboard,
                InlineKeyboardButtons = new()
                {
                    new()
                    {
                        new InlineButton("Yes", GeneralCommands.Confirm)
                    },
                    new()
                    {
                        new InlineButton("No", GeneralCommands.Cancel)
                    }
                }
            });
        }
    }
}
