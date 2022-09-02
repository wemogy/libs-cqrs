using Microsoft.Extensions.DependencyInjection;
using Wemogy.CQRS.UnitTests.TestApplication.Common.Contexts;

namespace Wemogy.CQRS.UnitTests.TestApplication;

public static class DependencyInjection
{
    public static void AddTestApplication(this IServiceCollection services)
    {
        services.AddCQRS();
        services.AddSingleton(new TestContext
        {
            TenantId = TestContext.DefaultTenantId
        });
    }
}
