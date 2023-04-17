using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Views;
using FinancialAdvisorTelegramBot.Models.Telegram;

namespace FinancialAdvisorTelegramBot.Bot.Commands
{
    public interface ICommand
    {
        static string? TEXT_STYLE { get; }

        static string? DEFAULT_STYLE { get; }

        /// <summary>
        /// Multitaskable commands are commands that have delayed IsFinished property and save their state in the database
        /// </summary>
        bool IsFinished => true;

        /// <summary>
        /// If true, the command will update context view after complete execution.
        /// If command is ContextMenu, property automatic set to false
        /// </summary>
        bool ShowContextMenuAfterExecution => false;

        bool CanExecute(UpdateArgs update, TelegramUser user);

        Task Execute(UpdateArgs update, TelegramUser user);

        bool IsCanceled(UpdateArgs update) => update.GetTextData() == GeneralCommands.Cancel;

        /// <summary>
        /// Context menu is a command that shows a list of available commands in current context to the user
        /// </summary>
        bool IsContextMenu(TelegramUser user) => false;
    }
}
