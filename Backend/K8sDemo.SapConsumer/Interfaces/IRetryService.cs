using K8sDemo.Shared.Models;

namespace K8sDemo.SapConsumer.Interfaces
{
    public interface IRetryService
    {
        bool CanRetry(MaterialPickedEvent evt);

        void IncreaseRetry(MaterialPickedEvent evt);
    }
}
