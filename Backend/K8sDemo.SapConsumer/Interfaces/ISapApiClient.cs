using K8sDemo.Shared.Models;

namespace K8sDemo.SapConsumer.Interfaces
{
    public interface ISapApiClient
    {
        Task<bool> PostMaterialPickedAsync(MaterialPickedEvent evt);
    }
}
