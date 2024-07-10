using System.Net;
using FastEndpoints;
using Wemogy.Core.Errors.Exceptions;
using Wemogy.Core.Errors.Extensions;
using Wemogy.Core.Json.ExceptionInformation;
using Wemogy.CQRS.Extensions.FastEndpoints.Extensions;

namespace Wemogy.CQRS.Extensions.FastEndpoints.PostProcessors;

public class CqrsEndpointExceptionPostProcessor<TRequest, TResponse> : IPostProcessor<TRequest, TResponse>
{
    public Task PostProcessAsync(IPostProcessorContext<TRequest, TResponse> context, CancellationToken ct)
    {
        if (!context.HasExceptionOccurred)
        {
            return Task.CompletedTask;
        }

        var exception = context.ExceptionDispatchInfo.SourceException;

        var statusCode = (exception as ErrorException)?.ErrorType.ToHttpStatusCode() ?? HttpStatusCode.InternalServerError;

        context.MarkExceptionAsHandled();

        context.HttpContext.Response.Headers.AppendJsonTypeHeader<ExceptionInformation>();
        return context.HttpContext.Response.SendStringAsync(exception.ToJson(), (int)statusCode, cancellation: ct);
    }
}
