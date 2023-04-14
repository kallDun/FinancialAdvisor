using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Models.Telegram;

namespace FinancialAdvisorTelegramBot.Bot.Commands
{
    public interface ICommand
    {
        static string? COMMAND_TEXT_STYLE { get; }

        static string? COMMAND_DEFAULT_STYLE { get; }

        bool IsFinished => true;

        bool CanExecute(UpdateArgs update, TelegramUser user);

        Task Execute(UpdateArgs update, TelegramUser user);

        bool IsCanceled(UpdateArgs update)
        {
            return update.GetTextData() == GeneralCommands.Cancel;
        }
    }
}
