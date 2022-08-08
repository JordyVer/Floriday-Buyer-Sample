using Axerrio.BB.AspNetCore.Infrastructure.ModelBinders.Request;
using Floriday_Buyer_Sample.Shared.Application.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcessingQueue.Infrastructure.Abstractions;

namespace Floriday_Buyer_Sample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;

        public TestController(ILogger<TestController> logger)
        {
            _logger = logger;
        }

        [HttpPost(Name = "TestResult")]
        [Authorize]
        public async Task<IResult> GetTestResultAsync([FromBody] CreateTestCommand command, [FromServices] IProcessingQueueItemPublisher publisher)
        {
            await publisher.PublishAsync(command.TestKey.ToString(), command, RequestId.Create());

            return Results.Ok("published command successfully");
        }

        [HttpGet("preprocess")]
        public async Task<IResult> GetRecordsForPreprocessing([FromServices] IProcessingQueueItemProcessing processor)
        {
            var items = await processor.GetEventsForPreprocessingAsync();
            return Results.Ok(items);
        }

        [HttpPost("skipped/{key}")]
        public async Task<IResult> MarkSkipped(int key, [FromServices] IProcessingQueueItemProcessing processor)
        {
            var items = await processor.GetEventsForPreprocessingAsync();
            var item = items.Where(i => i.ProcessingQueueItemKey == key).FirstOrDefault();
            await processor.MarkEventSkippedAsync(item);
            return Results.Ok();
        }
    }
}