using System.Threading.Tasks;

namespace Wemogy.CQRS.Queries.Abstractions;

public interface IQueryHandler<in TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    public Task<TResult> HandleAsync(TQuery query);
}
