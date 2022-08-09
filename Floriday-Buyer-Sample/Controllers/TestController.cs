using Axerrio.BB.AspNetCore.Infrastructure.ModelBinders.Request;
using Axerrio.BB.DDD.Domain.IntegrationEvents.Abstractions;
using Azure.Messaging.ServiceBus;
using Floriday_Buyer_Sample.Shared.Application.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcessingQueue.Infrastructure.Abstractions;
using System.Text.Json;

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
        public async Task<IResult> GetTestResultAsync([FromBody] CreateTestCommand command, [FromServices] IProcessingQueueItemPublisher<ServiceBusMessage> publisher)
        {
            // Mocked een servibus handler met tenant id en userid (in dit geval komt het van de http header.) 
            var integrationEvent = new ServiceBusMessage(JsonSerializer.Serialize(new PricelistCreatedIntegrationEvent(command.TestKey)));
            integrationEvent.ApplicationProperties.Add("x-tenant-id", HttpContext.Request.Headers["x-tenant-id"].ToString());
            integrationEvent.ApplicationProperties.Add("x-user-id", HttpContext.Request.Headers["x-user-id"].ToString());

            await publisher.PublishAsync(integrationEvent, "TestItem", command.TestKey.ToString(), command, RequestId.Create());

            return Results.Ok("published command successfully");
        }
    }

    public class PricelistCreatedIntegrationEvent : IntegrationEvent
    {
        public int PartyKey { get; }

        public PricelistCreatedIntegrationEvent(int partyKey)
        {
            PartyKey = partyKey;
        }
    }
}