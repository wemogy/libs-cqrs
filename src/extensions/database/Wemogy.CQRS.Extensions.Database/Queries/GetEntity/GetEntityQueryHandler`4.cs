using System;
using System.Threading.Tasks;
using Wemogy.CQRS.Queries.Abstractions;
using Wemogy.Infrastructure.Database.Core.Abstractions;

namespace Wemogy.CQRS.Extensions.Database.Queries.GetEntity
{
    public abstract class GetEntityQueryHandler<TQuery, TDatabaseRepository, TEntity, TPartitionKey>
        : GetEntityQueryHandler<TQuery, TDatabaseRepository, TEntity, TPartitionKey, Guid>
        where TQuery : GetEntityQuery<TEntity, TPartitionKey>
        where TDatabaseRepository : IDatabaseRepository<TEntity, TPartitionKey, Guid>
        where TEntity : IEntityBase<Guid>
        where TPartitionKey : IEquatable<TPartitionKey>
    {
        protected GetEntityQueryHandler(TDatabaseRepository databaseRepository)
            : base(databaseRepository)
        {
        }
    }
}
