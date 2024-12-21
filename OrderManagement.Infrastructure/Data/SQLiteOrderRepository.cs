using System.Data.SQLite;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using OrderManagement.Core.Abstraction;
using OrderManagement.Core.Abstraction.Commands;
using OrderManagement.Core.Implementation;
using OrderManagement.Core.Implementation.Commands;

namespace OrderManagement.Infrastructure.Data
{
    public class SQLiteOrderRepository : IOrderRepository
    {
        private readonly ICommandHandler _commandHandler;
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public SQLiteOrderRepository(IConfiguration configuration, ILogger logger)
        {
            _configuration = configuration;
            _logger = logger;
        }
        public SQLiteOrderRepository(ICommandHandler commandHandler, string connectionString)
        {
            _commandHandler = commandHandler ?? throw new ArgumentNullException(nameof(commandHandler));
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            ConnectionString = _connectionString; // Вс��ановлюємо властивість ConnectionString
            InitializeDatabase();
        }

        public SQLiteOrderRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _commandHandler = new CommandHandler(this); // Ініціалізуємо _commandHandler
            ConnectionString = _connectionString; // Встановлюємо властивість ConnectionString
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            var createTableCmd = connection.CreateCommand();
            createTableCmd.CommandText = @"
        CREATE TABLE IF NOT EXISTS Orders (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            TotalAmount REAL
        );
        CREATE TABLE IF NOT EXISTS OrderProducts (
            OrderId INTEGER,
            ProductId INTEGER,
            ProductName TEXT,
            ProductPrice REAL,
            FOREIGN KEY(OrderId) REFERENCES Orders(Id)
        );";
            createTableCmd.ExecuteNonQuery();
        }

        public void SaveOrder(Order order)
        {
            var saveCommand = new SaveOrderCommand(order);
            _commandHandler.Handle(saveCommand);
        }

        public Order GetOrderById(int id)
        {
            var getCommand = new GetOrderByIdCommand(id);
            _commandHandler.Handle(getCommand);
            return getCommand.Result;
        }

        public void UpdateOrder(Order order)
        {
            var updateCommand = new UpdateOrderCommand(order);
            _commandHandler.Handle(updateCommand);
        }

        public void DeleteOrder(int orderId)
        {
            var deleteCommand = new DeleteOrderCommand(orderId);
            _commandHandler.Handle(deleteCommand);
        }

        public string? ConnectionString { get; set; }
    }
}