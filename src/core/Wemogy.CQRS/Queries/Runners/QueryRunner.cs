using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Wemogy.Core.Errors;
using Wemogy.CQRS.Abstractions;
using Wemogy.CQRS.Common.ValueObjects;
using Wemogy.CQRS.Queries.Abstractions;

namespace Wemogy.CQRS.Queries.Runners;

public class QueryRunner<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    private readonly ICommandQueryDependencyResolver _commandQueryDependencyResolver;
    private readonly IQueryHandler<TQuery, TResult>? _queryHandler;
    private readonly IEnumerable<IQueryValidator<TQuery>> _queryValidators;
    private readonly IEnumerable<IQueryAuthorization<TQuery>> _queryAuthorizations;
    private readonly IRemoteQueryRunner<TQuery, TResult>? _remoteQueryRunner;

    public QueryRunner(
        ICommandQueryDependencyResolver commandQueryDependencyResolver,
        IEnumerable<IQueryValidator<TQuery>> queryValidators,
        IEnumerable<IQueryAuthorization<TQuery>> queryAuthorizations,
        IQueryHandler<TQuery, TResult>? queryHandler = null,
        IRemoteQueryRunner<TQuery, TResult>? remoteQueryRunner = null)
    {
        _commandQueryDependencyResolver = commandQueryDependencyResolver;
        _queryHandler = queryHandler;
        _queryValidators = queryValidators;
        _queryAuthorizations = queryAuthorizations;
        _remoteQueryRunner = remoteQueryRunner;
    }

    public async Task<TResult> QueryAsync(TQuery query, CancellationToken cancellationToken)
    {
        using var activity = Observability.DefaultActivities.StartActivity($"{typeof(TQuery).Name} Query");
        TResult result;

        if (_remoteQueryRunner == null)
        {
            foreach (var validator in _queryValidators)
            {
                using var validatorActivity = Observability.DefaultActivities.StartActivity($"{typeof(TQuery).Name} QueryValidator");
                validator.Validate(query);
            }

            foreach (var authorization in _queryAuthorizations)
            {
                using var authorizationActivity = Observability.DefaultActivities.StartActivity($"{typeof(TQuery).Name} QueryAuthorization");
                await authorization.AuthorizeAsync(query);
            }

            if (_queryHandler == null)
            {
                throw Error.Unexpected(
                    "QueryHandlerNotRegistered",
                    $"QueryHandler for {typeof(TQuery).Name} is not registered.");
            }

            using var queryHandlerActivity = Observability.DefaultActivities.StartActivity($"{typeof(TQuery).Name} QueryHandler");
            result = await _queryHandler.HandleAsync(query, cancellationToken);
            queryHandlerActivity?.Stop();
        }
        else
        {
            // build the QueryRequest
            var deps = _commandQueryDependencyResolver.ResolveDependencies();
            var queryRequest = new QueryRequest<TQuery>(
                query,
                deps);
            result = await _remoteQueryRunner.QueryAsync(queryRequest, cancellationToken);
        }

        return result;
    }
}
