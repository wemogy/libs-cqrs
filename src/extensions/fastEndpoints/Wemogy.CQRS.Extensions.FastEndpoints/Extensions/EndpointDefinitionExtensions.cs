using FastEndpoints;
using Wemogy.CQRS.Extensions.FastEndpoints.PostProcessors;

namespace Wemogy.CQRS.Extensions.FastEndpoints.Extensions;

public static class EndpointDefinitionExtensions
{
    public static void AddErrorHandlerPostProcessor(this EndpointDefinition endpointDefinition)
    {
        endpointDefinition.PostProcessor<ErrorHandlerPostProcessor>(Order.Before);
    }
}
