using System.Threading.Tasks;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.UnitTests.TestApplication.Common.Entities;

namespace Wemogy.CQRS.UnitTests.TestApplication.Commands.CreateUser;

public class CreateUserCommandPostProcessor : ICommandPostProcessor<CreateUserCommand, User>
{
    public static int CalledCount { get; private set; }
    public static User? PassedResult { get; private set; }

    public static void Reset()
    {
        CalledCount = 0;
        PassedResult = null;
    }

    public Task ProcessAsync(CreateUserCommand command, User result)
    {
        CalledCount++;
        PassedResult = result;
        return Task.CompletedTask;
    }
}
