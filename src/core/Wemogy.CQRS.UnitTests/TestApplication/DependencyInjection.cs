using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Wemogy.CQRS.Setup;
using Wemogy.CQRS.UnitTests.AssemblyA.Commands;
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
        return services.AddCQRS(
            new List<Assembly>()
            {
                Assembly.GetCallingAssembly(),
                Assembly.GetExecutingAssembly(),
                typeof(TrackUserActivityCommand).Assembly
            },
            new Dictionary<Type, Type>()
            {
                { typeof(TestContext), typeof(TestContext) },
            });
    }
}
