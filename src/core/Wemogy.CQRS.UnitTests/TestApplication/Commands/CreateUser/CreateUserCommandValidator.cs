using System.Threading.Tasks;
using FluentValidation;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.UnitTests.TestApplication.Commands.CreateUser;

public class CreateUserCommandValidator : FluentValidationCommandValidator<CreateUserCommand>, ICommandValidator<CreateUserCommand>
{
    public static int CalledCount { get; private set; }

    public static void Reset()
    {
        CalledCount = 0;
    }

    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Firstname).NotEmpty();
    }

    public new Task ValidateAsync(CreateUserCommand command)
    {
        CalledCount++;
        return base.ValidateAsync(command);
    }
}
