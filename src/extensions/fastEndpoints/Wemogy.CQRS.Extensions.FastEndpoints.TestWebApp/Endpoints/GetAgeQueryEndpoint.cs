using Wemogy.CQRS.Extensions.FastEndpoints.Endpoints;
using Wemogy.CQRS.Extensions.FastEndpoints.TestWebApp.Queries.GetAge;

namespace Wemogy.CQRS.Extensions.FastEndpoints.TestWebApp.Endpoints;

public class GetAgeQueryEndpoint : QueryEndpointBase<GetAgeQuery, int>
{
    public GetAgeQueryEndpoint()
    {
        EnableCircularDependencyToleration();
    }
}
