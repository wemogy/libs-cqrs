using System.Threading.Tasks;

namespace Wemogy.CQRS.Commands.Abstractions;

public interface ICommandHandler<in TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    public Task<TResult> HandleAsync(TCommand command);
}
