using System.Threading.Tasks;

namespace Wemogy.CQRS.Commands.Abstractions;

public interface ICommandPostProcessor<in TCommand, in TResult>
    where TCommand : ICommand<TResult>
{
    public Task ProcessAsync(TCommand command, TResult result);
}
