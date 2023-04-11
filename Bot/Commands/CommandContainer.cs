namespace FinancialAdvisorTelegramBot.Bot.Commands
{
    public class CommandContainer : ICommandContainer
    {
        public List<ICommand> Commands { get; }

        public CommandContainer(params ICommand[] commands)
        {
            Commands = commands.ToList();
        }
    }
}
