using System.Threading;
using System.Threading.Tasks;
using Wemogy.CQRS.Common.ValueObjects;
using Wemogy.CQRS.Queries.Abstractions;

namespace Wemogy.CQRS.Abstractions;

public interface IRemoteQueryRunner<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    Task<TResult> QueryAsync(QueryRequest<TQuery> query, CancellationToken cancellationToken);
}
