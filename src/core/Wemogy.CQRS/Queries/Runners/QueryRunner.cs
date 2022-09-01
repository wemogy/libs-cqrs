using System.Threading;
using System.Threading.Tasks;
using Wemogy.CQRS.Queries.Abstractions;

namespace Wemogy.CQRS.Queries.Runners;

public class QueryRunner<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    private readonly IQueryHandler<TQuery, TResult> _queryHandler;

    public QueryRunner(IQueryHandler<TQuery, TResult> queryHandler)
    {
        _queryHandler = queryHandler;
    }

    public async Task<TResult> QueryAsync(TQuery query, CancellationToken cancellationToken)
    {
        var result = await _queryHandler.HandleAsync(query, cancellationToken);
        return result;
    }
}
