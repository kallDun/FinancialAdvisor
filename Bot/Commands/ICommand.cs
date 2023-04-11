using FinancialAdvisorTelegramBot.Models.Telegram;
using Telegram.Bot.Types;

namespace FinancialAdvisorTelegramBot.Bot.Commands
{
    public interface ICommand
    {
        virtual bool IsFinished => true;

        bool CanExecute(Update update, TelegramUser user);

        Task Execute(Update update, TelegramUser user);
    }
}
