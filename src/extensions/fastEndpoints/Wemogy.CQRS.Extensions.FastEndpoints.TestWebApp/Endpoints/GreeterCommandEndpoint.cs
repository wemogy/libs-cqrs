using Wemogy.CQRS.Extensions.FastEndpoints.Endpoints;
using Wemogy.CQRS.Extensions.FastEndpoints.TestWebApp.Commands.Greeter;

namespace Wemogy.CQRS.Extensions.FastEndpoints.TestWebApp.Endpoints;

public class GreeterCommandEndpoint : CommandEndpointBase<GreeterCommand, string>
{
    public GreeterCommandEndpoint()
    {
        EnableCircularDependencyToleration();
    }
}
