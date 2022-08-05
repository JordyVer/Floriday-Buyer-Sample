using Floriday_Buyer_Sample.Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcessingQueue.Domain.Services.Abstractions;

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
        public async Task<IResult> GetTestResultAsync([FromBody] CreateTestCommand command, [FromServices] IMediator mediator, [FromServices] IProcessingQueueItemService processingQueueItemService)
        {
            // gets tenant id and tenantuser id

            // inserts record into processingqueue

            await processingQueueItemService.AddAsync(command.TestKey.ToString(), command);

            return await mediator.Send(command);
        }
    }
}