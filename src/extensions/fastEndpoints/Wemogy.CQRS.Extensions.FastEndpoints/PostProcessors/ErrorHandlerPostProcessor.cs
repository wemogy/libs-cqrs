using FastEndpoints;
using Wemogy.Core.Errors.Exceptions;
using Wemogy.Core.Errors.Extensions;
using Wemogy.Core.Json.ExceptionInformation;
using Wemogy.CQRS.Extensions.FastEndpoints.Extensions;

namespace Wemogy.CQRS.Extensions.FastEndpoints.PostProcessors;

public class ErrorHandlerPostProcessor : IGlobalPostProcessor
{
    public async Task PostProcessAsync(IPostProcessorContext context, CancellationToken ct)
    {
        if (!context.HasExceptionOccurred || context.IsExceptionHandled())
        {
            return;
        }

        var exception = context.ExceptionDispatchInfo.SourceException;

        if (exception is ErrorException errorException)
        {
            var statusCode = errorException.ErrorType.ToHttpStatusCode();

            context.MarkExceptionAsHandled();

            context.HttpContext.Response.Headers.AppendJsonTypeHeader<ExceptionInformation>();
            await context.HttpContext.Response.SendAsync(exception.ToJson(), (int)statusCode, cancellation: ct);
            return;
        }

        context.ExceptionDispatchInfo.Throw();
    }
}
