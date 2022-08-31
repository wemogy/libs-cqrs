using Wemogy.CQRS.Commands.Structs;

namespace Wemogy.CQRS.Commands.Abstractions;

public interface ICommandHandler<in TCommand> : ICommandHandler<TCommand, Void>
    where TCommand : ICommand<Void>
{
}
