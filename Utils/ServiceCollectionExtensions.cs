using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Bot.Updates;

namespace FinancialAdvisorTelegramBot.Utils
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AutomaticAddCommandsFromAssembly(this IServiceCollection services)
        {
            var commands = AppDomain
              .CurrentDomain
              .GetAssemblies()
              .SelectMany(assembly => assembly.GetTypes())
              .Where(type => typeof(ICommand).IsAssignableFrom(type))
              .Where(type => type.IsClass);

            foreach (var command in commands)
            {
                services.AddScoped(command);
            }

            services.AddScoped<ICommandContainer>(
                provider => new CommandContainer(
                    commands.Select(x => (ICommand)provider.GetRequiredService(x))
                    .ToArray()));

            return services;
        }

        public static IServiceCollection AutomaticAddUpdateListenersFromAssembly(this IServiceCollection services)
        {
            var listeners = AppDomain
              .CurrentDomain
              .GetAssemblies()
              .SelectMany(assembly => assembly.GetTypes())
              .Where(type => typeof(ITelegramUpdateListener).IsAssignableFrom(type))
              .Where(type => type.IsClass);

            foreach (var listener in listeners)
            {
                services.AddScoped(listener);
            }
            
            services.AddScoped<ITelegramAvailableListeners>(
                provider => new TelegramAvailableListeners(
                    listeners.Select(x => (ITelegramUpdateListener)provider.GetRequiredService(x))
                    .ToArray()));

            return services;
        }
    }
}
