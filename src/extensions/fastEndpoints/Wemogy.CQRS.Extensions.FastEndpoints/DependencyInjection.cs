using Wemogy.CQRS.Extensions.FastEndpoints.Setup;
using Wemogy.CQRS.Setup;

namespace Wemogy.CQRS.Extensions.FastEndpoints;

public static class DependencyInjection
{
    public static RemoteHttpServerSetupEnvironment AddRemoteHttpServer(
        this CQRSSetupEnvironment cqrsSetupEnvironment,
        Uri baseAddress)
    {
        return new RemoteHttpServerSetupEnvironment(cqrsSetupEnvironment, baseAddress);
    }

    public static RemoteHttpServerSetupEnvironment AddRemoteHttpServer(
        this CQRSSetupEnvironment cqrsSetupEnvironment,
        HttpClient httpClient)
    {
        return new RemoteHttpServerSetupEnvironment(cqrsSetupEnvironment, httpClient);
    }
}
