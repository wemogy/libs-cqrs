using Microsoft.Extensions.DependencyInjection;

namespace Wemogy.CQRS.UnitTests.AssemblyA;

public static class DependencyInjection
{
    public static void AddAssemblyA(this IServiceCollection services)
    {
        services.AddCQRS();
    }
}
