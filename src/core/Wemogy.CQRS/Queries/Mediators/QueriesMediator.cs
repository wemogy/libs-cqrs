using System.Threading;
using System.Threading.Tasks;
using Wemogy.CQRS.Queries.Abstractions;
using Wemogy.CQRS.Queries.Registries;

namespace Wemogy.CQRS.Queries.Mediators;

public class QueriesMediator : IQueries
{
    private readonly QueryRunnerRegistry _queryRunnerRegistry;

    public QueriesMediator(QueryRunnerRegistry queryRunnerRegistry)
    {
        _queryRunnerRegistry = queryRunnerRegistry;
    }

    public Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        return _queryRunnerRegistry.ExecuteQueryRunnerAsync(query, cancellationToken);
    }
}
