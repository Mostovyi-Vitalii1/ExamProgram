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

        public SQLiteOrderRepository(IConfiguration configuration, ILogger logger, Func<IOrderRepository> orderRepositoryFactory)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _commandHandler = new CommandHandler(orderRepositoryFactory ?? throw new ArgumentNullException(nameof(orderRepositoryFactory)));
            _connectionString = _configuration.GetConnectionString("SQLiteConnection") ?? throw new ArgumentNullException(nameof(_configuration));
            ConnectionString = _connectionString;
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
            if (order == null) throw new ArgumentNullException(nameof(order));
            var saveCommand = new SaveOrderCommand(order);
            _commandHandler.Handle(saveCommand);

            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            foreach (var product in order.Products)
            {
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO OrderProducts (OrderId, ProductId, ProductName, ProductPrice)
                    VALUES ($orderId, $productId, $productName, $productPrice);
                ";
                command.Parameters.AddWithValue("$orderId", order.Id);
                command.Parameters.AddWithValue("$productId", product.Id);
                command.Parameters.AddWithValue("$productName", product.Name);
                command.Parameters.AddWithValue("$productPrice", product.Price);
                command.ExecuteNonQuery();
            }
        }

        public Order GetOrderById(int id)
        {
            var getCommand = new GetOrderByIdCommand(id);
            _commandHandler.Handle(getCommand);
            var order = getCommand.Result;

            if (order != null)
            {
                using var connection = new SQLiteConnection(_connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT ProductId, ProductName, ProductPrice
                    FROM OrderProducts
                    WHERE OrderId = $orderId;
                ";
                command.Parameters.AddWithValue("$orderId", id);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var product = new Product(reader.GetInt32(0), reader.GetString(1), reader.GetDecimal(2));
                    order.AddProduct(product);
                }
            }

            return order;
        }

        public void UpdateOrder(Order order)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));
            var updateCommand = new UpdateOrderCommand(order);
            _commandHandler.Handle(updateCommand);

            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Orders
                SET TotalAmount = $totalAmount
                WHERE Id = $id;
            ";
            command.Parameters.AddWithValue("$totalAmount", order.TotalAmount);
            command.Parameters.AddWithValue("$id", order.Id);

            if (command.ExecuteNonQuery() == 0)
            {
                throw new Exception("Не вдалося оновити замовлення");
            }
        }

        public void DeleteOrder(int orderId)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                // Delete related products first
                var deleteProductsCommand = connection.CreateCommand();
                deleteProductsCommand.CommandText = @"
            DELETE FROM OrderProducts
            WHERE OrderId = $orderId;
        ";
                deleteProductsCommand.Parameters.AddWithValue("$orderId", orderId);
                deleteProductsCommand.ExecuteNonQuery();

                // Delete the order
                var deleteOrderCommand = connection.CreateCommand();
                deleteOrderCommand.CommandText = @"
            DELETE FROM Orders
            WHERE Id = $id;
        ";
                deleteOrderCommand.Parameters.AddWithValue("$id", orderId);

                if (deleteOrderCommand.ExecuteNonQuery() == 0)
                {
                    throw new Exception("Не вдалося видалити замовлення");
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        public string? ConnectionString { get; set; }
    }
}