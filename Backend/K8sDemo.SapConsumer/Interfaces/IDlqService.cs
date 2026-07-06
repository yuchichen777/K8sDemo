namespace K8sDemo.SapConsumer.Interfaces;

public interface IDlqService
{
    Task RequeueAsync(string workOrder, string reelId);
}