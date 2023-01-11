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

    public QueryRunner(
        IQueryHandler<TQuery, TResult> queryHandler,
        List<IQueryValidator<TQuery>> queryValidators)
    {
        _queryHandler = queryHandler;
        _queryValidators = queryValidators;
    }

    public async Task<TResult> QueryAsync(TQuery query, CancellationToken cancellationToken)
    {
        foreach (var validator in _queryValidators)
        {
            validator.Validate(query);
        }

        var result = await _queryHandler.HandleAsync(query, cancellationToken);
        return result;
    }
}
