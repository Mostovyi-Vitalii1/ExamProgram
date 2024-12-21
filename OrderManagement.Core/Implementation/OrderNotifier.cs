namespace OrderManagement.Core.Implementation
{
    public class OrderNotifier
    {
        public void NotifyOrderSaved(Order order)
        {
            Console.WriteLine($"Замовлення з ID {order.Id} збережено. Сума: {order.TotalAmount}");
            // Тут можна додати додаткову логіку для надсилання email, push-сповіщень тощо.
        }

        public void NotifyOrderUpdated(Order order)
        {
            Console.WriteLine($"Замовлення з ID {order.Id} оновлено. Сума: {order.TotalAmount}");
            // Додаткові дії, наприклад, сповіщення користувача про зміни
        }

        public void NotifyOrderDeleted(int orderId)
        {
            Console.WriteLine($"Замовлення з ID {orderId} було видалено.");
            // Надіслати повідомлення про видалення замовлення
        }
    }
}