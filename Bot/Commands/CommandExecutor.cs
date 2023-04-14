using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Updates;
using FinancialAdvisorTelegramBot.Bot.Views;
using FinancialAdvisorTelegramBot.Models.Telegram;

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

        public async Task GetUpdate(UpdateArgs update, TelegramUser user)
        {
            if (_storage == null) return;
            CommandExecutorData data = _storage.GetById(update.From.Id);

            if (data.CurrentCommand == null || data.CurrentCommand.IsFinished)
            {
                data.CurrentCommand = GetCommand(update, user);
            }
            else if (data.CurrentCommand.IsCanceled(update))
            {
                data.CurrentCommand = _defaultHelpCommand;
            }
            
            if (data.CurrentCommand != null)
            {
                await data.CurrentCommand.Execute(update, user);
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
            return _defaultHelpCommand;
        }
    }
}
