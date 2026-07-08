using K8sDemo.SapConsumer.Interfaces;
using K8sDemo.SapConsumer.Models;
using K8sDemo.Shared.Models;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace K8sDemo.SapConsumer.Services
{
    public class MaterialEventProcessor : IMaterialEventProcessor
    {
        private readonly ISapApiClient _sapApiClient;
        private readonly IRetryService _retryService;
        private readonly IStatisticsService _statisticsService;
        private readonly ILogger<MaterialEventProcessor> _logger;

        public MaterialEventProcessor(
            ISapApiClient sapApiClient,
            IRetryService retryService,
            IStatisticsService statisticsService,
            ILogger<MaterialEventProcessor> logger)
        {
            _sapApiClient = sapApiClient;
            _retryService = retryService;
            _statisticsService = statisticsService;
            _logger = logger;
        }

        public async Task<ProcessResult> ProcessAsync(MaterialPickedEvent evt)
        {
            var sw = Stopwatch.StartNew();

            try
            {
                var success = await _sapApiClient.PostMaterialPickedAsync(evt);

                sw.Stop();

                _logger.LogInformation("Process completed in {ElapsedMs} ms", sw.ElapsedMilliseconds);

                if (success)
                {
                    _statisticsService.RecordSuccess(evt, sw.Elapsed.TotalMilliseconds);
                    _logger.LogInformation("Material {WorkOrder}/{ReelId} uploaded successfully.", evt.WorkOrder, evt.ReelId);

                    return new ProcessResult
                    {
                        Success = true
                    };

                }

                return HandleFailure(evt, "Retry Limit Exceeded", sw.Elapsed.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                sw.Stop();

                _logger.LogError(ex, "Unexpected error while processing {WorkOrder}/{ReelId}", evt.WorkOrder, evt.ReelId);

                return HandleFailure(evt, "Exception Retry Limit Exceeded", sw.Elapsed.TotalMilliseconds);
            }
        }

        private ProcessResult HandleFailure(MaterialPickedEvent evt, string dlqMessage, double processMs)
        {
            if (_retryService.CanRetry(evt))
            {
                _logger.LogWarning("Retry {RetryCount} for {WorkOrder}/{ReelId}", evt.RetryCount + 1, evt.WorkOrder, evt.ReelId);

                _statisticsService.RecordRetry(evt, processMs);

                _retryService.IncreaseRetry(evt);

                return new ProcessResult
                {
                    ShouldRetry = true
                };
            }

            _logger.LogError("Moved to DLQ. WorkOrder={WorkOrder}, ReelId={ReelId}, Error={Error}", evt.WorkOrder, evt.ReelId, dlqMessage);

            _statisticsService.RecordDlq(evt, dlqMessage, processMs);

            return new ProcessResult
            {
                SendToDlq = true
            };
        }
    }
}
