using System.Threading.Tasks;

namespace Wemogy.CQRS.Commands.Abstractions;

public interface ICommandPostProcessor<in TCommand, in TResult>
    where TCommand : ICommandBase
{
    public Task ProcessAsync(TCommand command, TResult result);
}
