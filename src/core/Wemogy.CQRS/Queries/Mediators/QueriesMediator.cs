using System;
using System.Threading;
using System.Threading.Tasks;
using Wemogy.CQRS.Queries.Abstractions;
using Wemogy.CQRS.Queries.Registries;

namespace Wemogy.CQRS.Queries.Mediators;

public class QueriesMediator : IQueries
{
    private readonly QueryRunnerRegistry _queryRunnerRegistry;
    private readonly IServiceProvider _serviceProvider;

    public QueriesMediator(QueryRunnerRegistry queryRunnerRegistry, IServiceProvider serviceProvider)
    {
        _queryRunnerRegistry = queryRunnerRegistry;
        _serviceProvider = serviceProvider;
    }

    public Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        return _queryRunnerRegistry.ExecuteQueryRunnerAsync(_serviceProvider, query, cancellationToken);
    }
}
