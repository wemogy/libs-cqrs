using System;
using System.Threading.Tasks;
using Wemogy.CQRS.Queries.Abstractions;
using Wemogy.Infrastructure.Database.Core.Abstractions;

namespace Wemogy.CQRS.Extensions.Database.Queries.GetEntity
{
    public abstract class GetEntityQueryHandler<TQuery, TDatabaseRepository, TEntity>
        : GetEntityQueryHandler<TDatabaseRepository, TEntity, Guid, Guid, TQuery>
        where TQuery : GetEntityQuery<TEntity, Guid>
        where TDatabaseRepository : IDatabaseRepository<TEntity, Guid, Guid>
        where TEntity : IEntityBase<Guid>
    {
        protected GetEntityQueryHandler(TDatabaseRepository databaseRepository)
            : base(databaseRepository)
        {
        }
    }
}
