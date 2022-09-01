using Wemogy.CQRS.Queries.Abstractions;

namespace Wemogy.CQRS.Extensions.Database.Queries.GetEntity;

public abstract class GetEntityQuery<TEntity, TPartitionKey, TId> : IQuery<TEntity>
{
    public TId Id { get; }

    protected GetEntityQuery(TId id)
    {
        Id = id;
    }

    public abstract TPartitionKey? GetPartitionKey();
}
