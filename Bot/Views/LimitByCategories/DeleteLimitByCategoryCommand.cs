﻿using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;
using FinancialAdvisorTelegramBot.Services.Operations;
using FinancialAdvisorTelegramBot.Services.Telegram;
using FinancialAdvisorTelegramBot.Utils;
using FinancialAdvisorTelegramBot.Utils.CommandSerializing;

namespace FinancialAdvisorTelegramBot.Bot.Views.LimitByCategories
{
    public class DeleteLimitByCategoryCommand : ICommand
    {
        public static string TEXT_STYLE => "Delete limit";
        public static string DEFAULT_STYLE => "/delete";
        public virtual bool IsFinished { get; private set; } = false;
        public bool ShowContextMenuAfterExecution => true;

        [CommandPropertySerializable] public int Status { get; set; }

        private readonly IBot _bot;
        private readonly ITelegramUserService _telegramUserService;
        private readonly ILimitByCategoryService _limitByCategoryService;

        public DeleteLimitByCategoryCommand(IBot bot, ITelegramUserService telegramUserService, ILimitByCategoryService limitByCategoryService)
        {
            _bot = bot;
            _telegramUserService = telegramUserService;
            _limitByCategoryService = limitByCategoryService;
        }

        public bool CanExecute(UpdateArgs update, TelegramUser user)
        {
            var split = (string.IsNullOrEmpty(user.ContextMenu) ? string.Empty : user.ContextMenu).Split('/');
            return split.Length == 4 && split[0] == ContextMenus.Category && split[2] == ContextMenus.LimitByCategory
                && (update.GetTextData() == DEFAULT_STYLE
                || update.GetTextData() == TEXT_STYLE);
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
                decimal maxExpense = Converters.ToDecimal(splitContextMenu[3]);
                if (user.UserId is null) throw new InvalidOperationException("User id is null");
                await _limitByCategoryService.Delete(user.UserId.Value, categoryName, maxExpense);

                await _bot.Write(user, new TextMessageArgs
                {
                    Text = $"Limit <code>{maxExpense}</code> by category {categoryName} has been deleted",
                });

                await _telegramUserService.SetContextMenu(user, $"{ContextMenus.Category}/{categoryName}/{ContextMenus.LimitByCategory}");
            }
        }

        private async Task AskForDelete(TelegramUser user, string text, string[] splitContextMenu)
        {
            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Are you sure you want to delete limit <code>{splitContextMenu[3]}</code> by category {splitContextMenu[1]} with ALL data?",
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
