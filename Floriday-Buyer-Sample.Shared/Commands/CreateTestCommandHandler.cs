using Axerrio.BB.DDD.Application.Commands.Abstractions;
using Floriday_Buyer_Sample.Shared.Application.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Floriday_Buyer_Sample.Shared.Commands
{
    public class CreateTestCommandHandler : CommandHandler<CreateTestCommand>
    {
        private readonly ILogger<CreateTestCommandHandler> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public CreateTestCommandHandler(ILogger<CreateTestCommandHandler> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        }

        public override async Task<CommandResult> Handle(CreateTestCommand command, CancellationToken cancellationToken = default)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            await Task.Delay(100, cancellationToken);

            bool success = command.TestKey % 2 == 0;

            if (success)
                return new CommandResult.Ok(_logger, Guid.NewGuid(), 0);
            else
                throw new Exception("TestKey is not even exception");
        }
    }
}