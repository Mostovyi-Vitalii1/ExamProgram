using OrderManagement.Core.Abstraction.Commands;

namespace OrderManagement.Core.Implementation.Commands
{
    public class SaveOrderCommand : ICommand
    {
        public Order Order { get; }

        public SaveOrderCommand(Order order)
        {
            Order = order;
        }
    }
}