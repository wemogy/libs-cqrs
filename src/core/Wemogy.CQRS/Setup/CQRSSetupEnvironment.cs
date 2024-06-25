using Microsoft.Extensions.DependencyInjection;

namespace Wemogy.CQRS.Setup;

public class CQRSSetupEnvironment
{
    public IServiceCollection ServiceCollection { get; }

    public CQRSSetupEnvironment(IServiceCollection serviceCollection)
    {
        ServiceCollection = serviceCollection;
    }

    protected CQRSSetupEnvironment(CQRSSetupEnvironment setupEnvironment)
    {
        ServiceCollection = setupEnvironment.ServiceCollection;
    }
}
