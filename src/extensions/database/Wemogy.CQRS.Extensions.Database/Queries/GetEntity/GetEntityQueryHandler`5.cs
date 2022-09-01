using System;
using System.Threading;
using System.Threading.Tasks;
using Wemogy.CQRS.Queries.Abstractions;
using Wemogy.Infrastructure.Database.Core.Abstractions;

namespace Wemogy.CQRS.Extensions.Database.Queries.GetEntity;

public abstract class
    GetEntityQueryHandler<TDatabaseRepository, TEntity, TPartitionKey, TId, TQuery> : IQueryHandler<TQuery, TEntity>
    where TQuery : GetEntityQuery<TEntity, TPartitionKey, TId>
    where TDatabaseRepository : IDatabaseRepository<TEntity, TPartitionKey, TId>
    where TEntity : IEntityBase<TId>
    where TPartitionKey : IEquatable<TPartitionKey>
    where TId : IEquatable<TId>
{
    private readonly TDatabaseRepository _databaseRepository;

    protected GetEntityQueryHandler(TDatabaseRepository databaseRepository)
    {
        _databaseRepository = databaseRepository;
    }

    public Task<TEntity> HandleAsync(TQuery query, CancellationToken cancellationToken)
    {
        var partitionKeyValue = query.GetPartitionKey();

        if (partitionKeyValue == null)
        {
            return _databaseRepository.GetAsync(query.Id, cancellationToken);
        }

        return _databaseRepository.GetAsync(query.Id, partitionKeyValue, cancellationToken);
    }
}
