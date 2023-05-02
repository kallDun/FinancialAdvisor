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
              .Where(x => x.GetCustomAttributes(typeof(CustomRepositoryAttribute), true).Length > 0);

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
              .Where(x => x.GetCustomAttributes(typeof(CustomServiceAttribute), true).Length > 0);

            foreach (Type? customService in customServices)
            {
                if (customService is null) throw new Exception("Custom service not found");
                Type? customServiceInterfaceType = customService.GetInterfaces().First();                

                CustomServiceAttribute? attribute = Attribute.GetCustomAttribute(customService, typeof(CustomServiceAttribute)) as CustomServiceAttribute;
                if (attribute is null) throw new Exception("Custom service attribute not found");

                if (attribute is CustomBackgroundServiceAttribute) customServiceInterfaceType = null;

                switch (attribute.LifeTimeType)
                {
                    case LifeTimeServiceType.Scoped:
                        if (customServiceInterfaceType is null) services.AddScoped(customService);
                        else services.AddScoped(customServiceInterfaceType, customService);
                        break;
                    case LifeTimeServiceType.Singleton:
                        if (customServiceInterfaceType is null) services.AddSingleton(customService);
                        else services.AddSingleton(customServiceInterfaceType, customService);
                        break;
                    case LifeTimeServiceType.Transient:
                        if (customServiceInterfaceType is null) services.AddTransient(customService);
                        else services.AddTransient(customServiceInterfaceType, customService);
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
