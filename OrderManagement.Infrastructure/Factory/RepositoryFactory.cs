using Microsoft.Extensions.Configuration;
using OrderManagement.Core.Abstraction;
using OrderManagement.Core.Abstraction.Commands;
using OrderManagement.Core.Implementation;
using OrderManagement.Core.Implementation.Commands;
using OrderManagement.Infrastructure.Data;

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
            var logger = new ConsoleLogger(); // Assuming ConsoleLogger is used
            Func<IOrderRepository> repositoryFactory = null;
            repositoryFactory = () => new SQLiteOrderRepository(_configuration, logger, repositoryFactory);
            var commandHandler = new CommandHandler(repositoryFactory);
            var repository = repositoryFactory();
            return repository;
        }
    }
}