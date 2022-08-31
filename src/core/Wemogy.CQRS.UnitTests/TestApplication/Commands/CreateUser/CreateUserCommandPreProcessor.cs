using System.Threading.Tasks;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.UnitTests.TestApplication.Commands.CreateUser;

public class CreateUserCommandPreProcessor : ICommandPreProcessor<CreateUserCommand>
{
    public static int CalledCount { get; private set; }

    public static void Reset()
    {
        CalledCount = 0;
    }

    public Task ProcessAsync(CreateUserCommand command)
    {
        CalledCount++;
        return Task.CompletedTask;
    }
}
