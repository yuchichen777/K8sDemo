using K8sDemo.SapConsumer.Interfaces;
using K8sDemo.SapConsumer.Services;
using K8sDemo.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace K8sDemo.SapConsumer.Controllers
{
    [ApiController]
    [Route("api/dlq")]
    public class DlqController : ControllerBase
    {
        private readonly IDlqService _dlqService;
        private readonly IStatisticsService _statisticsService;

        public DlqController(IDlqService dlqService, IStatisticsService statisticsService)
        {
            _dlqService = dlqService;
            _statisticsService = statisticsService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_statisticsService.GetDlqMessages().OrderByDescending(x => x.Time).ToList());
        }

        [HttpPost("requeue")]
        public async Task<IActionResult> Requeue(RequeueRequest request)
        {
            await _dlqService.RequeueAsync(request.WorkOrder, request.ReelId);

            return Ok();
        }
    }
}
