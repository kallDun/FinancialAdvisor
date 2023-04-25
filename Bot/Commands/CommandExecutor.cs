using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Updates;
using FinancialAdvisorTelegramBot.Bot.Views;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Telegram;

namespace FinancialAdvisorTelegramBot.Bot.Commands
{
    public class CommandExecutor : ITelegramUpdateListener
    {
        private readonly ICommandContainer _commandContainer;
        private readonly ITelegramUserService _telegramUserService;

        public CommandExecutor(ICommandContainer commandContainer, ITelegramUserService telegramUserService)
        {
            _commandContainer = commandContainer;
            _telegramUserService = telegramUserService;
        }

        public async Task GetUpdate(UpdateArgs update, TelegramUser user)
        {
            try
            {
                ICommand? commandToExecute;

                if (user.CurrentCommand is null)
                {
                    commandToExecute = GetCommand(update, user);
                    commandToExecute ??= GetContextMenu(user);
                }
                else
                {
                    commandToExecute = _telegramUserService.GetCurrentCommand(user, _commandContainer);
                    if (commandToExecute != null && commandToExecute.IsCanceled(update))
                    {
                        commandToExecute = GetContextMenu(user);
                    }
                }

                if (commandToExecute is null) throw new ArgumentException($"Command or context not found. Try {MainMenuCommand.DEFAULT_STYLE}");

                await commandToExecute.Execute(update, user);
                if (commandToExecute.IsFinished)
                {
                    if (commandToExecute.ShowContextMenuAfterExecution)
                    {
                        commandToExecute = await ShowContextMenu(update, user);
                        if (commandToExecute is not null && commandToExecute.IsFinished)
                        {
                            commandToExecute = null;
                        }
                    }
                    else
                    {
                        commandToExecute = null;
                    }
                }
                await _telegramUserService.SaveCurrentCommand(user, commandToExecute);
            }
            catch (Exception)
            {
                await _telegramUserService.SaveCurrentCommand(user, null);
                throw;
            }
        }

        private ICommand? GetCommand(UpdateArgs update, TelegramUser user)
        {
            foreach (var command in _commandContainer.Commands)
            {
                if (command.CanExecute(update, user))
                {
                    return command;
                }
            }
            return null;
        }

        private ICommand? GetContextMenu(TelegramUser user)
        {
            if (user.ContextMenu is null) return null;
            foreach (var command in _commandContainer.Commands)
            {
                if (command.IsFinished && command.IsContextMenu(user.ContextMenu.Split('/')))
                {
                    return command;
                }
            }
            return null;
        }

        private async Task<ICommand?> ShowContextMenu(UpdateArgs update, TelegramUser user)
        {
            ICommand? commandToExecute = GetContextMenu(user);
            if (commandToExecute is not null)
            {
                await commandToExecute.Execute(update, user);
            }
            return commandToExecute;
        }
    }
}
