using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using OrderManagement.Core.Abstraction;
using OrderManagement.Core.Implementation;
using OrderManagement.Infrastructure.Factory;
using Xunit;

public class OrderRepositoryTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly string _connectionString = "Data Source=orders.db";

    public OrderRepositoryTests()
    {
        var configuration = Substitute.For<IConfiguration>();
        configuration.GetConnectionString("SQLiteConnection").Returns(_connectionString);

        var repositoryFactory = new RepositoryFactory(configuration);
        _orderRepository = repositoryFactory.CreateOrderRepository();

        InitializeDatabase();
    }

    // Метод для ініціалізації бази даних перед кожним тестом
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

    [Fact]
    public void Should_Save_Order_And_Generate_Id()
    {
        var order = new Order(0, 100);
        order.AddProduct(new Product(1, "Product 1", 50));
        order.AddProduct(new Product(2, "Product 2", 50));

        _orderRepository.SaveOrder(order);

        order.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Should_Get_Order_By_Generated_Id()
    {
        var order = new Order(0, 100);
        order.AddProduct(new Product(1, "Product 1", 50));
        order.AddProduct(new Product(2, "Product 2", 50));

        _orderRepository.SaveOrder(order);

        var savedOrder = _orderRepository.GetOrderById(order.Id);

        savedOrder.Should().NotBeNull();
        savedOrder.Id.Should().Be(order.Id);
        savedOrder.TotalAmount.Should().Be(order.TotalAmount);
    }

    [Fact]
    public void Should_Update_Order_TotalAmount()
    {
        var order = new Order(0, 100);
        order.AddProduct(new Product(1, "Product 1", 50));
        order.AddProduct(new Product(2, "Product 2", 50));

        _orderRepository.SaveOrder(order);

        order.TotalAmount = 200;
        _orderRepository.UpdateOrder(order);

        var updatedOrder = _orderRepository.GetOrderById(order.Id);

        updatedOrder.TotalAmount.Should().Be(200);
    }

    [Fact]
    public void Should_Delete_Order_By_Id()
    {
        var order = new Order(0, 100);
        order.AddProduct(new Product(1, "Product 1", 50));
        order.AddProduct(new Product(2, "Product 2", 50));

        _orderRepository.SaveOrder(order);

        _orderRepository.DeleteOrder(order.Id);

        var deletedOrder = _orderRepository.GetOrderById(order.Id);

        deletedOrder.Should().BeNull();
    }

    [Fact]
    public void Should_Save_And_Retrieve_Order_With_Products()
    {
        var order = new Order(0, 100);
        order.AddProduct(new Product(1, "Product 1", 50));
        order.AddProduct(new Product(2, "Product 2", 50));

        _orderRepository.SaveOrder(order);

        var retrievedOrder = _orderRepository.GetOrderById(order.Id);

        retrievedOrder.Should().NotBeNull();
        retrievedOrder.Products.Should().HaveCount(2);

        retrievedOrder.Products[0].Name.Should().Be("Product 1");
        retrievedOrder.Products[1].Name.Should().Be("Product 2");
    }

    [Fact]
    public void Should_Throw_Exception_When_Updating_Nonexistent_Order()
    {
        var nonExistentOrder = new Order(999, 100);

        var act = () => _orderRepository.UpdateOrder(nonExistentOrder);

        act.Should().Throw<Exception>().WithMessage("Не вдалося оновити замовлення");
    }

    [Fact]
    public void Should_Throw_Exception_When_Deleting_Nonexistent_Order()
    {
        var act = () => _orderRepository.DeleteOrder(999);

        act.Should().Throw<Exception>().WithMessage("Не вдалося видалити замовлення");
    }
}
