using Microsoft.Data.Sqlite;
using OrderManagement.Core.Abstraction;
using OrderManagement.Core.Implementation;

namespace OrderManagement.Infrastructure.Data;

public class SQLiteOrderRepository : IOrderRepository
{
    private readonly string _connectionString;

    public SQLiteOrderRepository(string connectionString)
    {
        _connectionString = connectionString;
        InitializeDatabase();
    }

    // Метод для ініціалізації бази даних
    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
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

    // Збереження замовлення
    public void SaveOrder(Order order)
    {
        InitializeDatabase(); // Переконайтеся, що таблиці ініціалізовані

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // Збереження замовлення
            var insertOrderCmd = connection.CreateCommand();
            insertOrderCmd.CommandText = "INSERT INTO Orders (TotalAmount) VALUES (@totalAmount)";
            insertOrderCmd.Parameters.AddWithValue("@totalAmount", order.TotalAmount);
            insertOrderCmd.ExecuteNonQuery();

            // Отримання Id замовлення
            var getOrderIdCmd = connection.CreateCommand();
            getOrderIdCmd.CommandText = "SELECT last_insert_rowid()";
            order.Id = (int)(long)getOrderIdCmd.ExecuteScalar();

            // Збереження продуктів в таблицю OrderProducts
            foreach (var product in order.Products)
            {
                var insertProductCmd = connection.CreateCommand();
                insertProductCmd.CommandText = "INSERT INTO OrderProducts (OrderId, ProductId, ProductName, ProductPrice) VALUES (@orderId, @productId, @productName, @productPrice)";
                insertProductCmd.Parameters.AddWithValue("@orderId", order.Id);
                insertProductCmd.Parameters.AddWithValue("@productId", product.Id);
                insertProductCmd.Parameters.AddWithValue("@productName", product.Name);
                insertProductCmd.Parameters.AddWithValue("@productPrice", product.Price);
                insertProductCmd.ExecuteNonQuery();
            }

            // Перевірка збереження даних
            var checkOrderCmd = connection.CreateCommand();
            checkOrderCmd.CommandText = "SELECT COUNT(*) FROM Orders WHERE Id = @id";
            checkOrderCmd.Parameters.AddWithValue("@id", order.Id);
            var count = (long)checkOrderCmd.ExecuteScalar();
            if (count == 0)
            {
                throw new Exception("Замовлення не було збережено в базі даних");
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Не вдалося зберегти замовлення", ex);
        }
    }

    // Отримання замовлення за Id
    public Order GetOrderById(int id)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // Отримання замовлення
            var selectOrderCmd = connection.CreateCommand();
            selectOrderCmd.CommandText = "SELECT Id, TotalAmount FROM Orders WHERE Id = @id";
            selectOrderCmd.Parameters.AddWithValue("@id", id);
            var reader = selectOrderCmd.ExecuteReader();

            if (reader.Read())
            {
                var order = new Order(reader.GetInt32(1))
                {
                    Id = reader.GetInt32(0)
                };

                // Отримання продуктів для цього замовлення
                var selectProductsCmd = connection.CreateCommand();
                selectProductsCmd.CommandText = "SELECT ProductId, ProductName, ProductPrice FROM OrderProducts WHERE OrderId = @orderId";
                selectProductsCmd.Parameters.AddWithValue("@orderId", id);
                var productsReader = selectProductsCmd.ExecuteReader();

                while (productsReader.Read())
                {
                    order.AddProduct(new Product(
                        productsReader.GetInt32(0),
                        productsReader.GetString(1),
                        productsReader.GetDecimal(2)
                    ));
                }

                return order;
            }

            return null;
        }
        catch (Exception ex)
        {
            throw new Exception("Не вдалося отримати замовлення", ex);
        }
    }

    // Оновлення замовлення
    public void UpdateOrder(Order order)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // Оновлення замовлення
            var updateOrderCmd = connection.CreateCommand();
            updateOrderCmd.CommandText = "UPDATE Orders SET TotalAmount = @totalAmount WHERE Id = @id";
            updateOrderCmd.Parameters.AddWithValue("@id", order.Id);
            updateOrderCmd.Parameters.AddWithValue("@totalAmount", order.TotalAmount);
            var rowsAffected = updateOrderCmd.ExecuteNonQuery();

            if (rowsAffected == 0)
            {
                throw new Exception("Не вдалося оновити замовлення");
            }

            // Оновлення продуктів у замовленні
            var deleteProductsCmd = connection.CreateCommand();
            deleteProductsCmd.CommandText = "DELETE FROM OrderProducts WHERE OrderId = @orderId";
            deleteProductsCmd.Parameters.AddWithValue("@orderId", order.Id);
            deleteProductsCmd.ExecuteNonQuery();

            // Додавання оновлених продуктів
            foreach (var product in order.Products)
            {
                var insertProductCmd = connection.CreateCommand();
                insertProductCmd.CommandText = "INSERT INTO OrderProducts (OrderId, ProductId, ProductName, ProductPrice) VALUES (@orderId, @productId, @productName, @productPrice)";
                insertProductCmd.Parameters.AddWithValue("@orderId", order.Id);
                insertProductCmd.Parameters.AddWithValue("@productId", product.Id);
                insertProductCmd.Parameters.AddWithValue("@productName", product.Name);
                insertProductCmd.Parameters.AddWithValue("@productPrice", product.Price);
                insertProductCmd.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Не вдалося оновити замовлення", ex);
        }
    }

    // Видалення замовлення
    public void DeleteOrder(int orderId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // Видалення продуктів замовлення
            var deleteProductsCmd = connection.CreateCommand();
            deleteProductsCmd.CommandText = "DELETE FROM OrderProducts WHERE OrderId = @orderId";
            deleteProductsCmd.Parameters.AddWithValue("@orderId", orderId);
            deleteProductsCmd.ExecuteNonQuery();

            // Видалення замовлення
            var deleteOrderCmd = connection.CreateCommand();
            deleteOrderCmd.CommandText = "DELETE FROM Orders WHERE Id = @orderId";
            deleteOrderCmd.Parameters.AddWithValue("@orderId", orderId);
            var rowsAffected = deleteOrderCmd.ExecuteNonQuery();

            if (rowsAffected == 0)
            {
                throw new Exception("Не вдалося видалити замовлення");
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Не вдалося видалити замовлення", ex);
        }
    }
}
