using Microsoft.Extensions.Configuration;
using OrderManagement.Core.Abstraction;
using OrderManagement.Core.Implementation;
using OrderManagement.Infrastructure.Factory;
using OrderManagement.Infrastructure;
using System.Data.SQLite;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        // Завантаження конфігурації
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        // Створення фабрики для логера та репозиторія
        var loggerFactory = new LoggerFactory(configuration);
        ILogger logger = loggerFactory.CreateLogger();

        var repositoryFactory = new RepositoryFactory(configuration);
        IOrderRepository orderRepository = repositoryFactory.CreateOrderRepository();

        string dbPath = "orders.db"; // Шлях до бази даних
        logger.Log($"Шлях до бази даних: {Path.GetFullPath(dbPath)}");

        // Перевірка наявності БД і створення її, якщо немає
        if (!File.Exists(dbPath))
        {
            SQLiteConnection.CreateFile(dbPath);
            logger.Log("База даних була створена.");
        }

        try
        {
            // Відкриття з'єднання і перевірка наявності таблиці sqlite_sequence перед скиданням значення
            using (var connection = new SQLiteConnection($"Data Source={dbPath}"))
            {
                connection.Open();
                var command = connection.CreateCommand();

                // Перевіряємо, чи існує таблиця sqlite_sequence
                command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='sqlite_sequence';";
                var tableExists = command.ExecuteScalar() != null;

                // Якщо таблиця існує, скидаємо значення автонумерації
                if (tableExists)
                {
                    command.CommandText = "DELETE FROM sqlite_sequence WHERE name='Orders';";
                    command.ExecuteNonQuery();
                    logger.Log("Автоновіруючі значення скинуті.");
                }
            }

            // Створення продуктів і замовлення
            var product1 = new Product(1, "Product A", 10.5m);
            var product2 = new Product(2, "Product B", 20.0m);

            var order = new OrderBuilder(101)
                .AddProduct(product1)
                .AddProduct(product2)
                .CalculateTotal()
                .Build();

            // Створення об'єкта OrderNotifier
            var orderNotifier = new OrderNotifier();

            // Збереження замовлення в БД
            orderRepository.SaveOrder(order);

            // Логування успішного збереження
            logger.Log($"Замовлення {order.Id} створено з сумою {order.TotalAmount}");
            
            // Сповіщення про збереження замовлення
            orderNotifier.NotifyOrderSaved(order);

            // Отримання замовлення з бази даних за ID
            var retrievedOrder = orderRepository.GetOrderById(order.Id);
            if (retrievedOrder != null)
            {
                logger.Log($"Отримано замовлення {retrievedOrder.Id} з сумою {retrievedOrder.TotalAmount}");
            }
            else
            {
                logger.Log($"Замовлення з ID {order.Id} не знайдено");
            }

            // Отримання попереднього замовлення
            var previousOrderId = order.Id - 1; // Припускаємо, що попереднє замовлення має ID - 1
            var previousOrder = orderRepository.GetOrderById(previousOrderId);

            if (previousOrder != null)
            {
                logger.Log($"Отримано попереднє замовлення {previousOrder.Id} з сумою {previousOrder.TotalAmount}");
            }
            else
            {
                logger.Log($"Попереднє замовлення з ID {previousOrderId} не знайдено");
            }

        }
        catch (Exception ex)
        {
            // Обробка помилки
            ExceptionHandling.HandleException(ex, logger);
        }
    }

    // Метод для створення таблиць бази даних

}
