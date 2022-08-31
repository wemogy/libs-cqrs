using System.Threading.Tasks;
using Wemogy.Core.Errors;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.UnitTests.TestApplication.Common.Contexts;

namespace Wemogy.CQRS.UnitTests.TestApplication.Commands.CreateUser;

public class CreateUserCommandAuthentication : ICommandAuthorization<CreateUserCommand>
{
    public static int CalledCount { get; private set; }

    public static void Reset()
    {
        CalledCount = 0;
    }

    private readonly TestContext _testContext;

    public CreateUserCommandAuthentication(TestContext testContext)
    {
        _testContext = testContext;
    }

    public Task AuthorizeAsync(CreateUserCommand command)
    {
        CalledCount++;
        if (_testContext.TenantId == TestContext.TenantAId)
        {
            throw Error.Authorization(
                "TenantUnauthorized",
                $"Tenant {TestContext.TenantAId} is not allowed to create users");
        }

        return Task.CompletedTask;
    }
}
