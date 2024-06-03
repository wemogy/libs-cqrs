using System.Threading;
using System.Threading.Tasks;
using Wemogy.CQRS.Queries.Abstractions;
using Wemogy.CQRS.UnitTests.AssemblyA.Queries;

namespace Wemogy.CQRS.UnitTests.TestApplication.Queries.GetUserActivity;

public class GetUserActivityQueryHandler : IQueryHandler<GetUserActivityQuery, int>
{
    public Task<int> HandleAsync(GetUserActivityQuery query, CancellationToken cancellationToken)
    {
        return Task.FromResult(1);
    }
}
