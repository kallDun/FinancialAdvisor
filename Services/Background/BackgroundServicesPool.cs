using FinancialAdvisorTelegramBot.Utils.Attributes;
using System.Reflection;

namespace FinancialAdvisorTelegramBot.Services.Background
{
    public class BackgroundServicesPool : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<Type, Timer> _backgroundServices = new();

        public BackgroundServicesPool(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            IEnumerable<Type> services = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetCustomAttribute<CustomBackgroundServiceAttribute>() != null
                && t.GetInterfaces().Contains(typeof(IHostedService)));

            _backgroundServices.Clear();
            foreach (Type type in services)
            {
                var attribute = type.GetCustomAttribute<CustomBackgroundServiceAttribute>();
                if (attribute is not null)
                {
                    var timer = new Timer((obj) => ExecuteServiceAsync(type, stoppingToken), null, TimeSpan.Zero, attribute.Delay);
                    _backgroundServices.Add(type, timer);
                }
            }

            return Task.CompletedTask;
        }

        
        private async void ExecuteServiceAsync(Type service, CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                try
                {
                    IHostedService? myScopedService = scope.ServiceProvider.GetRequiredService(service) as IHostedService;
                    if (myScopedService is not null)
                    {
                        await myScopedService.StartAsync(cancellationToken);
                    }
                }
                catch (Exception) { /*log*/ }
            }

            if (cancellationToken.IsCancellationRequested)
            {
                _backgroundServices[service].Change(Timeout.Infinite, 0);
            }
        }
    }
}
