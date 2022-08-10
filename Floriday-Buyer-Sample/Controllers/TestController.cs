using Axerrio.BB.AspNetCore.Infrastructure.ModelBinders.Request;
using Axerrio.BB.DDD.Domain.IntegrationEvents.Abstractions;
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
        public async Task<IResult> GetTestResultAsync([FromBody] CreateTestCommand command, [FromServices] IProcessingQueueItemPublisher<HttpContext> publisher)
        {
            await publisher.PublishAsync(HttpContext, "TestItem", command.TestKey.ToString(), command, RequestId.Create());

            return Results.Ok("published command successfully");
        }

        [HttpPost("servicebus")]
        [Authorize]
        public async Task<IResult> GetTestResultAsync([FromServices] IProcessingQueueItemPublisher<IntegrationEvent> publisher)
        {
            // Mocked een IE handler met tenant id en userid (in dit geval komt het van de http header.)
            // normal wordt dit in AzureServiceBusEventBus.Consumer al gezet op de tenantcontextaccesssor.
            var command = new CreateTestCommand();

            var integrationEvent = new TestIntegrationEvent();

            await publisher.PublishAsync(integrationEvent, "TestItem", command.TestKey.ToString(), command, RequestId.Create());

            return Results.Ok("published command successfully");
        }
    }

    public class TestIntegrationEvent : IntegrationEvent
    {
        public int TestKey { get; set; }
    }
}