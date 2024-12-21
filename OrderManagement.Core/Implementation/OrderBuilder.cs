namespace OrderManagement.Core.Implementation
{
    public class OrderBuilder
    {
        private readonly Order _order;

        public OrderBuilder(int orderId)
        {
            _order = new Order(orderId);
        }

        public OrderBuilder AddProduct(Product product)
        {
            _order.AddProduct(product);
            return this;
        }

        public OrderBuilder CalculateTotal()
        {
            _order.CalculateTotal();
            return this;
        }

        public Order Build()
        {
            return _order;
        }
    }
}
