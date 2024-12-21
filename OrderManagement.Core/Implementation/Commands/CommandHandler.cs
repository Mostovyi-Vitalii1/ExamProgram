using Microsoft.Data.Sqlite;
using OrderManagement.Core.Abstraction;
using OrderManagement.Core.Abstraction.Commands;

namespace OrderManagement.Core.Implementation.Commands
{
    public class CommandHandler : ICommandHandler
    {
        private readonly IOrderRepository _repository;

        public CommandHandler(IOrderRepository repository)
        {
            _repository = repository;
        }

        public void Handle(ICommand command)
        {
            // Ensure that the command handling logic does not call itself recursively
            if (command is SaveOrderCommand saveOrderCommand)
            {
                // Handle SaveOrderCommand
                SaveOrder(saveOrderCommand.Order);
            }
            else if (command is GetOrderByIdCommand getOrderByIdCommand)
            {
                // Handle GetOrderByIdCommand
                getOrderByIdCommand.Result = GetOrderById(getOrderByIdCommand.OrderId);
            }
            else if (command is UpdateOrderCommand updateOrderCommand)
            {
                // Handle UpdateOrderCommand
                UpdateOrder(updateOrderCommand.Order);
            }
            else if (command is DeleteOrderCommand deleteOrderCommand)
            {
                // Handle DeleteOrderCommand
                DeleteOrder(deleteOrderCommand.OrderId);
            }
            else
            {
                throw new NotSupportedException($"Command {command.GetType().Name} is not supported");
            }
        }

        private void SaveOrder(Order order)
        {
            using var connection = new SqliteConnection(_repository.ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Orders (TotalAmount)
                VALUES ($totalAmount);
                SELECT last_insert_rowid();
            ";
            command.Parameters.AddWithValue("$totalAmount", order.TotalAmount);

            order.Id = (int)(long)command.ExecuteScalar();
        }

        private Order GetOrderById(int id)
        {
            using var connection = new SqliteConnection(_repository.ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, TotalAmount
                FROM Orders
                WHERE Id = $id;
            ";
            command.Parameters.AddWithValue("$id", id);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Order(reader.GetInt32(0), reader.GetDecimal(1));
            }
            return null;
        }

        private void UpdateOrder(Order order)
        {
            using var connection = new SqliteConnection(_repository.ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Orders
                SET TotalAmount = $totalAmount
                WHERE Id = $id;
            ";
            command.Parameters.AddWithValue("$totalAmount", order.TotalAmount);
            command.Parameters.AddWithValue("$id", order.Id);

            command.ExecuteNonQuery();
        }

        private void DeleteOrder(int orderId)
        {
            using var connection = new SqliteConnection(_repository.ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                DELETE FROM Orders
                WHERE Id = $id;
            ";
            command.Parameters.AddWithValue("$id", orderId);

            command.ExecuteNonQuery();
        }

        public void Handle<TCommand>(TCommand command) where TCommand : ICommand
        {
            Handle((ICommand)command);
        }
    }
}