using System.Threading.Tasks;

namespace Wemogy.CQRS.Commands.Abstractions;

public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    public Task HandleAsync(TCommand command);
}
