using Wemogy.CQRS.Extensions.FastEndpoints.Endpoints;
using Wemogy.CQRS.Extensions.FastEndpoints.UnitTests.TestApplication.Queries.RequestTestContext;
using Wemogy.CQRS.Extensions.FastEndpoints.UnitTests.TestApplication.ValueObjects;

namespace Wemogy.CQRS.Extensions.FastEndpoints.UnitTests.TestApplication.Endpoints;

public class RequestTestContextQueryEndpoint : QueryEndpointBase<RequestTestContextQuery, TestContext>
{
}
