using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Updates;
using FinancialAdvisorTelegramBot.Bot.Views;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Telegram;

namespace FinancialAdvisorTelegramBot.Bot.Commands
{
    public class CommandExecutor : ITelegramUpdateListener
    {
        private readonly IBot _bot;
        private readonly ICommandContainer _commandContainer;
        private readonly ITelegramUserService _userService;
        private readonly HelpCommand _defaultHelpCommand;

        public CommandExecutor(IBot bot, ICommandContainer commandContainer, ITelegramUserService telegramUserService, HelpCommand defaultHelpCommand)
        {
            _bot = bot;
            _commandContainer = commandContainer;
            _userService = telegramUserService;
            _defaultHelpCommand = defaultHelpCommand;
        }

        public async Task GetUpdate(UpdateArgs update, TelegramUser user)
        {
            try
            {
                ICommand? currentCommand;

                if (user.CurrentView is null)
                {
                    currentCommand = GetCommand(update, user);
                }
                else
                {
                    currentCommand = _userService.GetCurrentCommand(user);
                    if (currentCommand != null && currentCommand.IsCanceled(update))
                    {
                        currentCommand = _defaultHelpCommand;
                    }
                }

                if (currentCommand != null)
                {
                    await currentCommand.Execute(update, user);
                    if (currentCommand.IsFinished)
                    {
                        currentCommand = null;
                    }
                    await _userService.SaveCurrentCommand(user, currentCommand);
                }
            }
            catch (Exception)
            {
                await _userService.SaveCurrentCommand(user, null);
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
            return _defaultHelpCommand;
        }
    }
}
