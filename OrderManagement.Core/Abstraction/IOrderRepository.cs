using OrderManagement.Core.Implementation;

namespace OrderManagement.Core.Abstraction
{
    public interface IOrderRepository
    {
        void SaveOrder(Order order);
        Order GetOrderById(int id);
        void UpdateOrder(Order order);
        void DeleteOrder(int orderId);
    }
}