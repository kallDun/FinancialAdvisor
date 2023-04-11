namespace FinancialAdvisorTelegramBot.Bot.Commands
{
    public interface ICommandContainer
    {
        List<ICommand> Commands { get; }
    }
}
