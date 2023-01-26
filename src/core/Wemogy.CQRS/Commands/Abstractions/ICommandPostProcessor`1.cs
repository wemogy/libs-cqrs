using System.Threading.Tasks;

namespace Wemogy.CQRS.Commands.Abstractions;

public interface ICommandPostProcessor<in TCommand>
    where TCommand : ICommand
{
    public Task ProcessAsync(TCommand command);
}
