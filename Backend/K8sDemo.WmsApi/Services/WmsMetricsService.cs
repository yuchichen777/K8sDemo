using System.Threading;

namespace K8sDemo.WmsApi.Services;

public class WmsMetricsService
{
    private long _publishedTotal;

    public long PublishedTotal => Interlocked.Read(ref _publishedTotal);

    public void RecordPublished()
    {
        Interlocked.Increment(ref _publishedTotal);
    }
}
