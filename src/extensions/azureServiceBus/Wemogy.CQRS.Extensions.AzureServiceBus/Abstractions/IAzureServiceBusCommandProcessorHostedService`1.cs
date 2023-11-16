using Microsoft.Extensions.Hosting;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.Extensions.AzureServiceBus.Abstractions
{
    public interface IAzureServiceBusCommandProcessorHostedService<TCommand> : IHostedService
        where TCommand : ICommandBase
    {
    }
}
