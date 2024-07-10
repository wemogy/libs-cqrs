using FastEndpoints;
using Wemogy.CQRS.Extensions.FastEndpoints.PostProcessors;

namespace Wemogy.CQRS.Extensions.FastEndpoints.Extensions;

public static class EndpointDefinitionExtensions
{
    public static void AddErrorHandlerPostProcessor(this EndpointDefinition endpointDefinition)
    {
        // Add the global ErrorHandlerPostProcessor after the endpoint post processors are executed
        endpointDefinition.PostProcessor<ErrorHandlerPostProcessor>(Order.After);
    }
}
