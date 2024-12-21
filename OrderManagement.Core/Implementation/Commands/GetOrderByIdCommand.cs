using OrderManagement.Core.Abstraction.Commands;

namespace OrderManagement.Core.Implementation.Commands
{
    public class GetOrderByIdCommand : ICommand
    {
        public int OrderId { get; }
        public Order Result { get; set; }

        public GetOrderByIdCommand(int orderId)
        {
            OrderId = orderId;
        }
    }
}