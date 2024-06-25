using Microsoft.Extensions.DependencyInjection;

namespace Wemogy.CQRS.UnitTests.AssemblyB;

public static class DependencyInjection
{
    public static void AddAssemblyB(this IServiceCollection services)
    {
        services.AddCQRS();
    }
}
