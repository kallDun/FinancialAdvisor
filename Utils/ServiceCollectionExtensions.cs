using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Bot.Updates;
using FinancialAdvisorTelegramBot.Utils.Attributes;

namespace FinancialAdvisorTelegramBot.Utils
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AutomaticAddCustomRepositoriesFromAssembly(this IServiceCollection services)
        {
            var repositories = AppDomain
              .CurrentDomain
              .GetAssemblies()
              .SelectMany(assembly => assembly.GetTypes())
              .Where(x => x.GetCustomAttributes(typeof(CustomRepositoryAttribute), false).Length > 0);

            foreach (var repository in repositories)
            {
                Type? repositoryInterfaceType = repository.GetInterfaces().First();
                if (repositoryInterfaceType is null) throw new Exception("Repository interface not found");
                services.AddScoped(repositoryInterfaceType, repository);
            }
            return services;
        }

        public static IServiceCollection AutomaticAddCustomServicesFromAssembly(this IServiceCollection services)
        {
            var customServices = AppDomain
              .CurrentDomain
              .GetAssemblies()
              .SelectMany(assembly => assembly.GetTypes())
              .Where(x => x.GetCustomAttributes(typeof(CustomServiceAttribute), false).Length > 0);

            foreach (var customService in customServices)
            {
                Type? customServiceInterfaceType = customService.GetInterfaces().First();
                if (customServiceInterfaceType is null) throw new Exception("Custom service interface not found");

                CustomServiceAttribute? attribute = Attribute.GetCustomAttribute(customService, typeof(CustomServiceAttribute)) as CustomServiceAttribute;
                if (attribute is null) throw new Exception("Custom service attribute not found");

                switch (attribute.LifeTimeType)
                {
                    case LifeTimeServiceType.Scoped:
                        services.AddScoped(customServiceInterfaceType, customService);
                        break;
                    case LifeTimeServiceType.Singleton:
                        services.AddSingleton(customServiceInterfaceType, customService);
                        break;
                    case LifeTimeServiceType.Transient:
                        services.AddTransient(customServiceInterfaceType, customService);
                        break;
                    default:
                        goto case LifeTimeServiceType.Scoped;
                }
            }
            return services;
        }

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
