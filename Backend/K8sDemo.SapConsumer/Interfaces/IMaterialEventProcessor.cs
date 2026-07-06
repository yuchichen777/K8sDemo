using K8sDemo.SapConsumer.Models;
using K8sDemo.Shared.Models;

namespace K8sDemo.SapConsumer.Interfaces
{
    public interface IMaterialEventProcessor
    {
        Task<ProcessResult> ProcessAsync(MaterialPickedEvent evt);
    }
}
