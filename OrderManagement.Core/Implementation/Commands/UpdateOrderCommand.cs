using OrderManagement.Core.Abstraction.Commands;

namespace OrderManagement.Core.Implementation.Commands
{
    public class UpdateOrderCommand : ICommand
    {
        public Order Order { get; }

        public UpdateOrderCommand(Order order)
        {
            Order = order;
        }
    }
}