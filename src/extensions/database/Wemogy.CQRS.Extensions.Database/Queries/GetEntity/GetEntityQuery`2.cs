using System;

namespace Wemogy.CQRS.Extensions.Database.Queries.GetEntity;

public abstract class GetEntityQuery<TEntity, TPartitionKey> : GetEntityQuery<TEntity, TPartitionKey, Guid>
{
    protected GetEntityQuery(Guid id)
        : base(id)
    {
    }
}
