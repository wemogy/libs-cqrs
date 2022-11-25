using Wemogy.CQRS.Extensions.Database.Queries.GetEntity;
using Wemogy.CQRS.Extensions.Database.UnitTests.TestApplication.Entities;

namespace Wemogy.CQRS.Extensions.Database.UnitTests.TestApplication.Queries.GetUser;

public class GetUserQuery : GetEntityQuery<User>
{
    public string TenantId { get; }

    public GetUserQuery(string id, string tenantId)
        : base(id)
    {
        TenantId = tenantId;
    }

    public override string GetPartitionKey()
    {
        return TenantId;
    }
}
