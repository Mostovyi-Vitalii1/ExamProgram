using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Core.Abstraction.Commands;
using OrderManagement.Core.Implementation.Commands;

namespace OrderManagement.Infrastructure.Factory
{
    public class ServiceProviderFactory
    {
        public static ServiceProvider CreateServiceProvider()
        {
            var serviceCollection = new ServiceCollection();

            // Register commands
            serviceCollection.AddTransient<ICommand, GetOrderByIdCommand>();
            serviceCollection.AddTransient<ICommand, SaveOrderCommand>();
            serviceCollection.AddTransient<ICommand, UpdateOrderCommand>();

            return serviceCollection.BuildServiceProvider();
        }
    }
}