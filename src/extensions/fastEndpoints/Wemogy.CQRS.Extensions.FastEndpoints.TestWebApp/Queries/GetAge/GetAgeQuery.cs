using Wemogy.CQRS.Queries.Abstractions;

namespace Wemogy.CQRS.Extensions.FastEndpoints.TestWebApp.Queries.GetAge;

public class GetAgeQuery : IQuery<int>
{
    public string Name { get; }

    public GetAgeQuery(string name)
    {
        Name = name;
    }
}
