using Wemogy.CQRS.Queries.Abstractions;

namespace Wemogy.CQRS.Extensions.FastEndpoints.TestWebApp.Queries.GetAge;

public class GetAgeQueryHandler : IQueryHandler<GetAgeQuery, int>
{
    public Task<int> HandleAsync(GetAgeQuery query, CancellationToken cancellationToken)
    {
        return Task.FromResult(18);
    }
}
