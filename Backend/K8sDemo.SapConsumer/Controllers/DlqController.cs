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

        public DlqController(IDlqService dlqService)
        {
            _dlqService = dlqService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(
                ConsumerStatistics.DlqMessages
                    .OrderByDescending(x => x.Time)
                    .ToList()
            );
        }

        [HttpPost("requeue")]
        public async Task<IActionResult> Requeue(RequeueRequest request)
        {
            await _dlqService.RequeueAsync(request.WorkOrder, request.ReelId);

            return Ok();
        }
    }
}
