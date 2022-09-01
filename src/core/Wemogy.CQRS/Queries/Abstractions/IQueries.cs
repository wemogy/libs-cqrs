using System.Threading;
using System.Threading.Tasks;

namespace Wemogy.CQRS.Queries.Abstractions;

public interface IQueries
{
    public Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
}
