namespace FinancialAdvisorTelegramBot.Bot.Commands
{
    public class CommandExecutorStorage
    {
        public Dictionary<long, CommandExecutorData> Commands { get; set; } = new();

        public CommandExecutorData GetById(long id)
        {
            if (Commands.ContainsKey(id))
            {
                return Commands[id];
            }
            else
            {
                Commands.Add(id, new CommandExecutorData());
                return Commands[id];
            }
        }
    }

    public class CommandExecutorData
    {
        public ICommand? CurrentCommand { get; set; }
    }
}
