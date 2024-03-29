using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Wemogy.CQRS.Queries.Abstractions;

namespace Wemogy.CQRS.Queries.Runners;

public class QueryRunner<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    private readonly IQueryHandler<TQuery, TResult> _queryHandler;
    private readonly List<IQueryValidator<TQuery>> _queryValidators;
    private readonly List<IQueryAuthorization<TQuery>> _queryAuthorizations;

    public QueryRunner(
        IQueryHandler<TQuery, TResult> queryHandler,
        List<IQueryValidator<TQuery>> queryValidators,
        List<IQueryAuthorization<TQuery>> queryAuthorizations)
    {
        _queryHandler = queryHandler;
        _queryValidators = queryValidators;
        _queryAuthorizations = queryAuthorizations;
    }

    public async Task<TResult> QueryAsync(TQuery query, CancellationToken cancellationToken)
    {
        using var activity = Observability.DefaultActivities.StartActivity($"{typeof(TQuery).Name} Query");
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

        using var queryHandlerActivity = Observability.DefaultActivities.StartActivity($"{typeof(TQuery).Name} QueryHandler");
        var result = await _queryHandler.HandleAsync(query, cancellationToken);
        queryHandlerActivity?.Stop();

        return result;
    }
}
