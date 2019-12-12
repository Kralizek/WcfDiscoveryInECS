using System;
using System.ServiceModel;
using Microsoft.Extensions.Logging;

namespace EMG.WcfService
{
    [ServiceContract]
    public interface ITestService
    {
        [OperationContract]
        string Echo(string message);
    }

    [ServiceBehavior(AddressFilterMode = AddressFilterMode.Any)]
    public class TestService : ITestService
    {
        private readonly ILogger<TestService> _logger;

        public TestService(ILogger<TestService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string Echo(string message)
        {
            _logger.LogInformation("Received {MESSAGE}", message);
            return message;
        }
    }
}
