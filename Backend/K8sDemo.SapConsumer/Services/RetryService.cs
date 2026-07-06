using K8sDemo.SapConsumer.Interfaces;
using K8sDemo.Shared.Models;

namespace K8sDemo.SapConsumer.Services
{
    public class RetryService : IRetryService
    {
        private const int MaxRetryCount = 3;

        public bool CanRetry(MaterialPickedEvent evt)
        {
            return evt.RetryCount < MaxRetryCount;
        }

        public void IncreaseRetry(MaterialPickedEvent evt)
        {
            evt.RetryCount++;
        }
    }
}
