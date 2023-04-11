using FinancialAdvisorTelegramBot.Models.Telegram;
using Telegram.Bot.Types;

namespace FinancialAdvisorTelegramBot.Bot.Commands
{
    public interface ICommand
    {
        string Name { get; }

        virtual bool IsFinished => true;

        Task Execute(Update update, TelegramUser user);
    }
}
