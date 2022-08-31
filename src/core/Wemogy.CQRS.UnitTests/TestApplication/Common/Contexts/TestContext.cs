using System;

namespace Wemogy.CQRS.UnitTests.TestApplication.Common.Contexts;

public class TestContext
{
    public static Guid DefaultTenantId { get; } = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public static Guid TenantAId { get; } = Guid.Parse("00000000-0000-0000-0000-000000000002");

    public Guid TenantId { get; set; }
}
