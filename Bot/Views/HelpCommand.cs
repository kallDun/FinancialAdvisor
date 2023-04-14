﻿using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Bot.ReplyArgs;
using FinancialAdvisorTelegramBot.Bot.Views.Profile;
using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;

namespace FinancialAdvisorTelegramBot.Bot.Views
{
    public class HelpCommand : ICommand
    {
        public static string COMMAND_TEXT_STYLE => "Help";
        public static string COMMAND_DEFAULT_STYLE => GeneralCommands.Help;

        private readonly IBot _bot;
        private readonly IUserService _userService;

        public HelpCommand(IBot bot, IUserService userService)
        {
            _bot = bot;
            _userService = userService;
        }

        public bool CanExecute(UpdateArgs update, TelegramUser user) 
            => update.GetTextData() == COMMAND_DEFAULT_STYLE
            || update.GetTextData() == COMMAND_TEXT_STYLE;

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            User? profile = user.UserId != null
                ? await _userService.GetById((int)user.UserId) : null;

            IEnumerable<string> buttons = profile == null
                ? new List<string>()
                {
                    OpenProfileMenuCommand.COMMAND_TEXT_STYLE
                }
                : new List<string>()
                {
                    StartCommand.COMMAND_DEFAULT_STYLE
                };

            await _bot.SendInlineKeyboard(user, new InlineKeyboardArgs
            {
                Text = "<b>↓ Available commands ↓</b>",
                Buttons = buttons,
                Placeholder = "Type command"
            });
        }
    }
}
