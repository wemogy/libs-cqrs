using Microsoft.Extensions.Hosting;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.Abstractions;

public interface IDelayedCommandProcessorHostedService<TCommand> : IHostedService
    where TCommand : ICommandBase
{
    /// <summary>
    /// Determines whether the processor has a running receiver that receives and processes messages.
    /// </summary>
    public bool IsAlive { get; }
}
