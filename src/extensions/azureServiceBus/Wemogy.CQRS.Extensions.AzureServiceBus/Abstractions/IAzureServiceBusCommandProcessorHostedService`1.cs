using Wemogy.CQRS.Abstractions;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.Extensions.AzureServiceBus.Abstractions
{
    public interface IAzureServiceBusCommandProcessorHostedService<TCommand> : IDelayedCommandProcessorHostedService<TCommand>
        where TCommand : ICommandBase
    {
    }
}
