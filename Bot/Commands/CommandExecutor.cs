using FinancialAdvisorTelegramBot.Bot.Updates;
using FinancialAdvisorTelegramBot.Bot.Views;
using FinancialAdvisorTelegramBot.Models.Telegram;
using Telegram.Bot.Types;

namespace FinancialAdvisorTelegramBot.Bot.Commands
{
    public class CommandExecutor : ITelegramUpdateListener
    {
        private static CommandExecutorStorage? _storage;        
        private readonly ICommandContainer _commandContainer;
        private readonly HelpCommand _defaultHelpCommand;

        public CommandExecutor(ICommandContainer commandContainer, HelpCommand defaultHelpCommand)
        {
            _commandContainer = commandContainer;
            _defaultHelpCommand = defaultHelpCommand;
            if (_storage == null) _storage = new();
        }

        public async Task GetUpdate(Update update, TelegramUser user)
        {
            if (_storage == null || update?.Message == null) return;
            CommandExecutorData data = _storage.GetById(update.Message.Chat.Id);

            if (data.CurrentCommand == null || data.CurrentCommand.IsFinished)
            {
                data.CurrentCommand = GetCommand(update, user);
            }
            if (data.CurrentCommand != null)
            {
                await data.CurrentCommand.Execute(update, user);
            }
        }

        private ICommand? GetCommand(Update update, TelegramUser user)
        {
            foreach (var command in _commandContainer.Commands)
            {
                if (command.CanExecute(update, user))
                {
                    return command;
                }
            }
            return _defaultHelpCommand;
        }
    }
}
