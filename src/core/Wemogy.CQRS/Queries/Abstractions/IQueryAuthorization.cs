using System.Threading.Tasks;

namespace Wemogy.CQRS.Queries.Abstractions;

public interface IQueryAuthorization<in TQuery>
    where TQuery : IQueryBase
{
    Task AuthorizeAsync(TQuery query);
}
