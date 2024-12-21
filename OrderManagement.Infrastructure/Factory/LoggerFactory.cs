using Microsoft.Extensions.Configuration;
using OrderManagement.Core.Abstraction;
using OrderManagement.Core.Implementation;

namespace OrderManagement.Infrastructure.Factory
{
    public class LoggerFactory
    {
        private readonly IConfiguration _configuration;

        public LoggerFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ILogger CreateLogger()
        {
            var loggerType = _configuration["Logging:LoggerType"];
            if (loggerType == "File")
            {
                return new FileLogger();
            }
            else
            {
                return new ConsoleLogger();
            }
        }
    }
}