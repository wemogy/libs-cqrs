using System;

namespace Wemogy.CQRS.Extensions.Database.Queries.GetEntity
{
    public abstract class GetEntityQuery<TEntity> : GetEntityQuery<TEntity, Guid>
    {
        protected GetEntityQuery(Guid id)
            : base(id)
        {
        }
    }
}
