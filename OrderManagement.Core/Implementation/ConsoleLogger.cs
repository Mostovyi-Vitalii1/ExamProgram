using OrderManagement.Core.Abstraction;

namespace OrderManagement.Core.Implementation
{
    public class ConsoleLogger : ILogger
    {
        public void Log(string message)
        {
            Console.WriteLine($"Console Logger: {message}");
        }
    }
}