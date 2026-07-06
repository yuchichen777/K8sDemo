using K8sDemo.SapConsumer.Interfaces;
using K8sDemo.SapConsumer.Models;
using K8sDemo.Shared.Models;
using System.Diagnostics;

namespace K8sDemo.SapConsumer.Services
{
    public class MaterialEventProcessor : IMaterialEventProcessor
    {
        private readonly ISapApiClient _sapApiClient;
        private readonly IRetryService _retryService;

        public MaterialEventProcessor(
            ISapApiClient sapApiClient,
            IRetryService retryService)
        {
            _sapApiClient = sapApiClient;
            _retryService = retryService;
        }

        public async Task<ProcessResult> ProcessAsync(MaterialPickedEvent evt)
        {
            var sw = Stopwatch.StartNew();

            try
            {
                var success = await _sapApiClient.PostMaterialPickedAsync(evt);

                sw.Stop();

                ConsumerStatistics.AddProcessTime(sw.Elapsed.TotalMilliseconds);

                Console.WriteLine($"[SapConsumer] ProcessTime={sw.ElapsedMilliseconds}ms");

                if (success)
                {
                    ConsumerStatistics.SuccessCount++;
                    ConsumerStatistics.AddEventTime();
                    ConsumerStatistics.AddLog(new Shared.Models.EventLog
                    {
                        Time = DateTime.UtcNow,
                        WorkOrder = evt.WorkOrder,
                        ReelId = evt.ReelId,
                        Result = "Success"
                    });
                    ConsumerStatistics.AddTrend();

                    return new ProcessResult
                    {
                        Success = true
                    };
                }

                return HandleFailure(evt, "Retry Limit Exceeded");
            }
            catch
            {
                sw.Stop();

                ConsumerStatistics.AddProcessTime(sw.Elapsed.TotalMilliseconds);

                return HandleFailure(evt, "Exception Retry Limit Exceeded");
            }
        }

        private ProcessResult HandleFailure(
            MaterialPickedEvent evt,
            string dlqMessage)
        {
            if (_retryService.CanRetry(evt))
            {
                ConsumerStatistics.RetryCount++;
                ConsumerStatistics.AddEventTime();
                ConsumerStatistics.AddLog(new Shared.Models.EventLog
                {
                    Time = DateTime.UtcNow,
                    WorkOrder = evt.WorkOrder,
                    ReelId = evt.ReelId,
                    Result = $"Retry {evt.RetryCount + 1}"
                });
                ConsumerStatistics.AddTrend();

                _retryService.IncreaseRetry(evt);

                return new ProcessResult
                {
                    ShouldRetry = true
                };
            }

            ConsumerStatistics.FailCount++;
            ConsumerStatistics.DlqCount++;
            ConsumerStatistics.AddEventTime();
            ConsumerStatistics.AddLog(new Shared.Models.EventLog
            {
                Time = DateTime.UtcNow,
                WorkOrder = evt.WorkOrder,
                ReelId = evt.ReelId,
                Result = "DLQ"
            });
            ConsumerStatistics.AddTrend();

            ConsumerStatistics.DlqMessages.Add(new DlqMessage
            {
                WorkOrder = evt.WorkOrder,
                ReelId = evt.ReelId,
                Material = evt.Material,
                Qty = evt.Qty,
                RetryCount = evt.RetryCount,
                Time = DateTime.UtcNow,
                ErrorMessage = dlqMessage
            });

            return new ProcessResult
            {
                SendToDlq = true
            };
        }
    }
}
