using System.Collections.Generic;
using Wemogy.CQRS.Queries.Abstractions;

namespace Wemogy.CQRS.Common.ValueObjects;

public class QueryRequest<TQuery>
    where TQuery : IQueryBase
{
    public TQuery Query { get; set; }

    public List<CommandQueryDependency> Dependencies { get; set; }

    public QueryRequest(
        TQuery query,
        List<CommandQueryDependency> dependencies)
    {
        Query = query;
        Dependencies = dependencies;
    }
}
