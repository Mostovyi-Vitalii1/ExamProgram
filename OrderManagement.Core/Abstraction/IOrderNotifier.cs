using OrderManagement.Core.Implementation;

namespace OrderManagement.Core.Abstraction
{
    public interface IOrderNotifier
    {
        void Update(Order order);
    }
}