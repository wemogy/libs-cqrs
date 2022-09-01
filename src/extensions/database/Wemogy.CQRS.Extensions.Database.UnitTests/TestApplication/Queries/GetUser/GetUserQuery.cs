using System;
using Wemogy.CQRS.Extensions.Database.Queries.GetEntity;
using Wemogy.CQRS.Extensions.Database.UnitTests.TestApplication.Entities;

namespace Wemogy.CQRS.Extensions.Database.UnitTests.TestApplication.Queries.GetUser;

public class GetUserQuery : GetEntityQuery<User>
{
    public Guid TenantId { get; }

    public GetUserQuery(Guid id, Guid tenantId)
        : base(id)
    {
        TenantId = tenantId;
    }

    public override Guid GetPartitionKey()
    {
        return TenantId;
    }
}
