using System.Threading;

namespace K8sDemo.SapApi.Services;

public class SapMetricsService
{
    private long _requestsTotal;
    private long _successTotal;
    private long _failureTotal;

    public long RequestsTotal => Interlocked.Read(ref _requestsTotal);
    public long SuccessTotal => Interlocked.Read(ref _successTotal);
    public long FailureTotal => Interlocked.Read(ref _failureTotal);

    public void RecordResult(bool success)
    {
        Interlocked.Increment(ref _requestsTotal);

        if (success)
        {
            Interlocked.Increment(ref _successTotal);
            return;
        }

        Interlocked.Increment(ref _failureTotal);
    }
}
