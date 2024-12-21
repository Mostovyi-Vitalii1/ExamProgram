using OrderManagement.Core.Abstraction;
using OrderManagement.Core.Abstraction.Commands;
using OrderManagement.Core.Implementation.Commands;
using OrderManagement.Infrastructure.Data;
using Microsoft.Extensions.Configuration;

namespace OrderManagement.Infrastructure.Factory
{
    public class RepositoryFactory
    {
        private readonly IConfiguration _configuration;

        public RepositoryFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IOrderRepository CreateOrderRepository()
        {
            string connectionString = _configuration.GetConnectionString("SQLiteConnection");
            var repository = new SQLiteOrderRepository(connectionString);
            var commandHandler = new CommandHandler(repository);
            return new SQLiteOrderRepository(commandHandler, connectionString);
        }
    }
}