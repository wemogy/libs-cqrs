using System.Threading.Tasks;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.UnitTests.TestApplication.Common.Entities;

namespace Wemogy.CQRS.UnitTests.TestApplication.Commands.CreateUser;

public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, User>
{
    public static int CalledCount { get; private set; }
    public static User? CreatedUser { get; private set; }
    public static void Reset()
    {
        CalledCount = 0;
        CreatedUser = null;
    }

    public Task<User> HandleAsync(CreateUserCommand command)
    {
        CalledCount++;
        CreatedUser = new User
        {
            Firstname = command.Firstname
        };
        return Task.FromResult(CreatedUser);
    }
}
