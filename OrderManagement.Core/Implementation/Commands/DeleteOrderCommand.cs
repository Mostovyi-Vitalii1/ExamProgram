using OrderManagement.Core.Abstraction.Commands;

namespace OrderManagement.Core.Implementation.Commands
{
    public class DeleteOrderCommand : ICommand
    {
        public int OrderId { get; }

        public DeleteOrderCommand(int orderId)
        {
            OrderId = orderId;
        }
    }
}