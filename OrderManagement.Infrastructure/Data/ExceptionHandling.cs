using System;
using OrderManagement.Core.Abstraction;

namespace OrderManagement.Infrastructure
{
    public static class ExceptionHandling
    {
        public static void HandleException(Exception ex, ILogger logger)
        {
            // Логування помилки
            logger.Log($"Помилка: {ex.Message}");

            // Можна додавати додаткову логіку, наприклад, запис у файл або сповіщення
            // Для більш серйозних ситуацій можна додавати механізми повторної спроби або аварійного завершення
        }
    }
}