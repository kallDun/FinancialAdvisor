using FinancialAdvisorTelegramBot.Bot.Updates;
using FinancialAdvisorTelegramBot.Models.Telegram;
using Telegram.Bot.Types;

namespace FinancialAdvisorTelegramBot.Bot.Commands
{
    public class CommandExecutor : ITelegramUpdateListener
    {
        private readonly ICommandContainer _commandContainer;

        private ICommand? _currentCommand;

        public CommandExecutor(ICommandContainer commandContainer)
        {
            _commandContainer = commandContainer;
        }

        public async Task GetUpdate(Update update, TelegramUser user)
        {
            if (_currentCommand == null || _currentCommand.IsFinished)
            {
                _currentCommand = GetCommand(update, user);
            }
            if (_currentCommand != null)
            {
                await _currentCommand.Execute(update, user);
            }
        }

        private ICommand? GetCommand(Update update, TelegramUser user)
        {
            Message? msg = update.Message;
            if (msg?.Text == null) return null;

            foreach (var command in GetAvailableCommands(user))
            {
                if (command.Name == msg.Text)
                {
                    return command;
                }
            }
            return null;
        }

        private IEnumerable<ICommand> GetAvailableCommands(TelegramUser user)
        {
            return _commandContainer.Commands;
        }
    }
}
