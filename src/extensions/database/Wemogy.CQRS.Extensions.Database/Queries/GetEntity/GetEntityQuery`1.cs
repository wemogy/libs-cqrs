using Wemogy.CQRS.Queries.Abstractions;

namespace Wemogy.CQRS.Extensions.Database.Queries.GetEntity;

public abstract class GetEntityQuery<TEntity> : IQuery<TEntity>
{
    public string Id { get; }

    protected GetEntityQuery(string id)
    {
        Id = id;
    }

    public abstract string? GetPartitionKey();
}
