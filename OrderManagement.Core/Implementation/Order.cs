namespace OrderManagement.Core.Implementation
{
    public class Order
    {
        public int Id { get; set; }
        public List<Product> Products { get; set; }
        public decimal TotalAmount { get; set; }

        // Конструктор для ініціалізації через Builder
        public Order(int id, decimal totalAmount)
        {
            Id = id;
            TotalAmount = totalAmount;
            Products = new List<Product>();
        }

        // Конструктор за замовчуванням (для випадків без Id)
        public Order(decimal totalAmount)
        {
            TotalAmount = totalAmount;
            Products = new List<Product>();
        }

        public void AddProduct(Product product)
        {
            Products.Add(product);
        }

        public void CalculateTotal()
        {
            TotalAmount = Products.Sum(p => p.Price);
        }
    }
}