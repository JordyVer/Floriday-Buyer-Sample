using Axerrio.BB.DDD.Application.Commands.Abstractions;

namespace Floriday_Buyer_Sample.Application.Commands
{
    public class CreateTestCommand : Command<CreateTestCommand>
    {
        public int TestKey { get; set; }

        protected override IEnumerable<object> GetMemberValues()
        {
            yield return TestKey;
        }
    }

    public class CreateTestCommandHandler : CommandHandler<CreateTestCommand>
    {
        public override async Task<CommandResult> Handle(CreateTestCommand command, CancellationToken cancellationToken = default)
        {
            await Task.Delay(100, cancellationToken);
            return new CommandResult.Ok(null, Guid.NewGuid(), 0);
        }
    }
}