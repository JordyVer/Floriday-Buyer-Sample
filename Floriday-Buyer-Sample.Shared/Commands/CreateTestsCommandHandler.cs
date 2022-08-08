using Axerrio.BB.DDD.Application.Commands.Abstractions;
using Floriday_Buyer_Sample.Shared.Application.Commands;
using Microsoft.Extensions.Logging;

namespace Floriday_Buyer_Sample.Shared.Commands
{
    public class CreateTestCommandHandler : CommandHandler<CreateTestCommand>
    {
        private readonly ILogger<CreateTestCommandHandler> _logger;

        public CreateTestCommandHandler(ILogger<CreateTestCommandHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override Task<CommandResult> Handle(CreateTestCommand command, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new CommandResult.Ok(_logger, requestId: null, 0) as CommandResult);
        }

        //public override async Task<CommandResult> Handle(CreateTestCommand command, CancellationToken cancellationToken = default)
        //{
        //    await Task.Delay(100, cancellationToken);

        //    bool success = (new Random().Next(10) > 5);

        //    if (success)
        //        return new CommandResult.Ok(_logger, Guid.NewGuid(), 0);
        //    else
        //        return new CommandResult.Failed(_logger, Guid.NewGuid());
        //}
    }
}