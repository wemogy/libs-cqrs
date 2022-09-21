using System.Threading.Tasks;
using Wemogy.CQRS.Commands.Structs;

namespace Wemogy.CQRS.Commands.Abstractions;

public abstract class CommandPostProcessor<TCommand> : ICommandPostProcessor<TCommand, Void>
    where TCommand : ICommandBase
{
    public Task ProcessAsync(TCommand command, Void result)
    {
        return ProcessAsync(command);
    }

    protected abstract Task ProcessAsync(TCommand command);
}
