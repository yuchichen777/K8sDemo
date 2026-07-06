using K8sDemo.Shared.Models;

namespace K8sDemo.SapApi.Services;

public class SapService
{
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
            return new SapResult
            {
                Success = false,
                Message = "SAP 模擬回傳失敗"
            };
        }

        if (evt.Message == "RETRY")
        {
            if (evt.RetryCount < 2)
            {
                return new SapResult
                {
                    Success = false,
                    Message = $"Retry {evt.RetryCount} Failed"
                };
            }

            return new SapResult
            {
                Success = true,
                Message = "Retry Success",
                SapDocumentNo = $"SAP{DateTime.Now:yyyyMMddHHmmss}"
            };
        }

        return new SapResult
        {
            Success = true,
            Message = "SAP 模擬上傳成功",
            SapDocumentNo = $"SAP{DateTime.Now:yyyyMMddHHmmss}"
        };
    }
}

public class SapResult
{
    public bool Success { get; set; }

    public string Message { get; set; } = "";

    public string? SapDocumentNo { get; set; }
}