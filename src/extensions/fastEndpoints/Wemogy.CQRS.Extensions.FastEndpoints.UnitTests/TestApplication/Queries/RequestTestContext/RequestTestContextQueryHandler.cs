using Wemogy.CQRS.Extensions.FastEndpoints.UnitTests.TestApplication.ValueObjects;
using Wemogy.CQRS.Queries.Abstractions;

namespace Wemogy.CQRS.Extensions.FastEndpoints.UnitTests.TestApplication.Queries.RequestTestContext;

public class RequestTestContextQueryHandler : IQueryHandler<RequestTestContextQuery, TestContext>
{
    public static readonly Stack<TestContext> ContextHistory = new Stack<TestContext>();

    private readonly TestContext _testContext;

    public RequestTestContextQueryHandler(TestContext testContext)
    {
        _testContext = testContext;
    }

    public Task<TestContext> HandleAsync(RequestTestContextQuery query, CancellationToken cancellationToken)
    {
        ContextHistory.Push(_testContext);
        return Task.FromResult(_testContext);
    }
}
