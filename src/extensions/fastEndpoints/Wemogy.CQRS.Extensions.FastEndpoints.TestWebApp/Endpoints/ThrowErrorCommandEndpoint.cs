using Wemogy.CQRS.Extensions.FastEndpoints.Endpoints;
using Wemogy.CQRS.Extensions.FastEndpoints.TestWebApp.Commands.ThrowError;

namespace Wemogy.CQRS.Extensions.FastEndpoints.TestWebApp.Endpoints;

public class ThrowErrorCommandEndpoint : CommandEndpointBase<ThrowErrorCommand>
{
    public ThrowErrorCommandEndpoint()
    {
        EnableCircularDependencyToleration();
    }
}
