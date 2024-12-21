using OrderManagement.Core.Abstraction;

namespace OrderManagement.Core.Implementation
{
    public class FileLogger : ILogger
    {
        private readonly string _filePath = "log.txt";

        public void Log(string message)
        {
            File.AppendAllText(_filePath, $"{DateTime.Now}: {message}{Environment.NewLine}");
        }
    }
}