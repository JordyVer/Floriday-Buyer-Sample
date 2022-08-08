using Axerrio.BB.DDD.Application.Commands.Abstractions;
using FluentValidation;

namespace Floriday_Buyer_Sample.Shared.Application.Commands
{
    public class CreateTestCommandValidator : AbstractValidator<CreateTestCommand>
    {
        public CreateTestCommandValidator()
        {
        }
    }

    public class CreateTestCommand : Command<CreateTestCommand>
    {
        public int TestKey { get; set; }

        protected override IEnumerable<object> GetMemberValues()
        {
            yield return TestKey;
        }
    }
}