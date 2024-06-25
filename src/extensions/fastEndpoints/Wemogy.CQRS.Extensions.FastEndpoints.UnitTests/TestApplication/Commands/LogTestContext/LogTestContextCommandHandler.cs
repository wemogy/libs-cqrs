using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Extensions.FastEndpoints.UnitTests.TestApplication.ValueObjects;

namespace Wemogy.CQRS.Extensions.FastEndpoints.UnitTests.TestApplication.Commands.LogTestContext;

public class LogTestContextCommandHandler : ICommandHandler<LogTestContextCommand>
{
    public static readonly Stack<TestContext> LogHistory = new Stack<TestContext>();

    private readonly TestContext _testContext;

    public LogTestContextCommandHandler(TestContext testContext)
    {
        _testContext = testContext;
    }

    public Task HandleAsync(LogTestContextCommand command)
    {
        LogHistory.Push(_testContext);
        return Task.CompletedTask;
    }
}
