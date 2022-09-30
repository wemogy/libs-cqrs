using Microsoft.Extensions.DependencyInjection;
using Wemogy.CQRS.Setup;
using Wemogy.CQRS.UnitTests.TestApplication.Common.Contexts;

namespace Wemogy.CQRS.UnitTests.TestApplication;

public static class DependencyInjection
{
    public static CQRSSetupEnvironment AddTestApplication(this IServiceCollection services)
    {
        services.AddSingleton(new TestContext
        {
            TenantId = TestContext.DefaultTenantId
        });
        return services.AddCQRS();
    }
}
