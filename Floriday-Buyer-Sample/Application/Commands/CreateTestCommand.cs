using MediatR;

namespace Floriday_Buyer_Sample.Application.Commands
{
    public class CreateTestCommand : IRequest<IResult>
    {
        public int TestKey { get; set; }
    }

    public class CreateTestCommandHandler : IRequestHandler<CreateTestCommand, IResult>
    {
        async Task<IResult> IRequestHandler<CreateTestCommand, IResult>.Handle(CreateTestCommand request, CancellationToken cancellationToken)
        {
            await Task.Delay(100, cancellationToken);

            return Results.NotFound("this result");
        }
    }
}