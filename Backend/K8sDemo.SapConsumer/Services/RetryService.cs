using K8sDemo.SapConsumer.Interfaces;
using K8sDemo.SapConsumer.Options;
using K8sDemo.Shared.Models;
using Microsoft.Extensions.Options;

namespace K8sDemo.SapConsumer.Services
{
    public class RetryService : IRetryService
    {
        private readonly RetryOptions _options;

        public RetryService(IOptions<RetryOptions> options)
        {
            _options = options.Value;
        }

        public bool CanRetry(MaterialPickedEvent evt)
        {
            return evt.RetryCount < _options.MaxRetryCount;
        }

        public void IncreaseRetry(MaterialPickedEvent evt)
        {
            evt.RetryCount++;
        }
    }
}
