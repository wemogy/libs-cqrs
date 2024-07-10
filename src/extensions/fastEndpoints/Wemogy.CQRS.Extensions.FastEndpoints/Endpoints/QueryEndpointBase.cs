using CaseExtensions;
using FastEndpoints;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Wemogy.Core.Errors;
using Wemogy.Core.Extensions;
using Wemogy.CQRS.Abstractions;
using Wemogy.CQRS.Common.ValueObjects;
using Wemogy.CQRS.Extensions.FastEndpoints.PostProcessors;
using Wemogy.CQRS.Queries.Abstractions;
using Wemogy.CQRS.Setup;

namespace Wemogy.CQRS.Extensions.FastEndpoints.Endpoints;

public class QueryEndpointBase<TQuery, TResult> : Endpoint<QueryRequest<TQuery>, TResult>
    where TQuery : IQuery<TResult>
{
    private bool _circularDependencyTolerationEnabled;

    protected void EnableCircularDependencyToleration()
    {
        _circularDependencyTolerationEnabled = true;
    }

    public override void Configure()
    {
        Verbs(Http.POST);
        var queryName = typeof(TQuery).Name.RemoveTrailingString("Query").ToKebabCase();
        Routes($"/api/queries/{queryName}");

        // ToDo: remove this
        AllowAnonymous();
        PostProcessor<CqrsEndpointExceptionPostProcessor<QueryRequest<TQuery>, TResult>>();
    }

    public override async Task HandleAsync(QueryRequest<TQuery> req, CancellationToken ct)
    {
        var logger = HttpContext.RequestServices.GetRequiredService<ILogger<QueryEndpointBase<TQuery, TResult>>>();
        var serviceCollection = HttpContext.RequestServices.GetRequiredService<CQRSSetupEnvironment>();
        var services = new ServiceCollection();

        foreach (var serviceDescriptor in serviceCollection.ServiceCollection)
        {
            // check, if serviceDescriptor is IRemoteQueryRunner<TCommand> to avoid circular dependency
            if (serviceDescriptor.ServiceType.IsGenericType &&
                serviceDescriptor.ServiceType.GetGenericTypeDefinition() == typeof(IRemoteQueryRunner<,>) &&
                serviceDescriptor.ServiceType.GetGenericArguments()[0] == typeof(TQuery))
            {
                if (_circularDependencyTolerationEnabled)
                {
                    logger.LogWarning(
                        "Circular dependency detected. Skipping IRemoteQueryRunner<{QueryName}>",
                        typeof(TQuery).Name);
                    continue;
                }

                throw Error.Unexpected(
                    "CircularDependency",
                    $"Circular dependency detected. IRemoteQueryRunner<{typeof(TQuery).Name}> is not allowed");
            }

            services.Add(serviceDescriptor);
        }

        services.AddCommandQueryDependencies(req.Dependencies);

        var serviceProvider = services.BuildServiceProvider();
        var queries = serviceProvider.GetRequiredService<IQueries>();

        var result = await queries.QueryAsync(req.Query, ct);

        await SendOkAsync(result, ct);
    }
}
