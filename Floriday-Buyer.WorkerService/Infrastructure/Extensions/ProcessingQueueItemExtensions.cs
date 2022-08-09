using Axerrio.BB.DDD.Application.Commands.Abstractions;
using Microsoft.AspNetCore.Mvc;
using ProcessingQueue.Domain.ProcessingQueueItems;
using System.Diagnostics;
using System.Text.Json;

namespace Floriday_Buyer.WorkerService.Infrastructure.Extensions
{
    public static class ProcessingQueueItemExtensions
    {
        public static string CreateErrorMessage(this ProcessingQueueItem item, CommandResult result)
        {
            return $"Failed {item.EventName} - {JsonSerializer.Serialize(result.Result<ProblemDetails>())}";
        }

        public static string CreateErrorMessage(this ProcessingQueueItem item, Exception exc)
        {
            return $"Failed {item.EventName} - {exc.Demystify()}";
        }
    }
}