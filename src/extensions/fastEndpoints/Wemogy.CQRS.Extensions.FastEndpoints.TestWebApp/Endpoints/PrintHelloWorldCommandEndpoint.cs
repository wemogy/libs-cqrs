using Wemogy.CQRS.Extensions.FastEndpoints.Endpoints;
using Wemogy.CQRS.Extensions.FastEndpoints.TestWebApp.Commands.PrintHelloWorld;

namespace Wemogy.CQRS.Extensions.FastEndpoints.TestWebApp.Endpoints;

public class PrintHelloWorldCommandEndpoint : CommandEndpointBase<PrintHelloWorldCommand>
{
    public PrintHelloWorldCommandEndpoint()
    {
        EnableCircularDependencyToleration();
    }
}
