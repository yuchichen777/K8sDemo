using K8sDemo.Shared.Models;

namespace K8sDemo.SapApi.Services;

public class SapService
{
    private readonly SapMetricsService _metrics;

    public SapService(SapMetricsService metrics)
    {
        _metrics = metrics;
    }

    public async Task<SapResult> PostMaterialPickedAsync(MaterialPickedEvent evt)
    {
        Console.WriteLine("[SapApi] 模擬上傳 SAP");
        Console.WriteLine($"WorkOrder: {evt.WorkOrder}");
        Console.WriteLine($"Material: {evt.Material}");
        Console.WriteLine($"ReelId: {evt.ReelId}");
        Console.WriteLine($"Qty: {evt.Qty}");

        await Task.Delay(500);

        if (evt.Message == "FAIL")
        {
            return Record(new SapResult
            {
                Success = false,
                Message = "SAP 模擬回傳失敗"
            });
        }

        if (evt.Message == "RETRY")
        {
            if (evt.RetryCount < 2)
            {
                return Record(new SapResult
                {
                    Success = false,
                    Message = $"Retry {evt.RetryCount} Failed"
                });
            }

            return Record(new SapResult
            {
                Success = true,
                Message = "Retry Success",
                SapDocumentNo = $"SAP{DateTime.Now:yyyyMMddHHmmss}"
            });
        }

        return Record(new SapResult
        {
            Success = true,
            Message = "SAP 模擬上傳成功",
            SapDocumentNo = $"SAP{DateTime.Now:yyyyMMddHHmmss}"
        });
    }

    private SapResult Record(SapResult result)
    {
        _metrics.RecordResult(result.Success);

        return result;
    }
}

public class SapResult
{
    public bool Success { get; set; }

    public string Message { get; set; } = "";

    public string? SapDocumentNo { get; set; }
}
