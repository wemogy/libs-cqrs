using System.Threading.Tasks;

namespace Wemogy.CQRS.Commands.Abstractions;

public interface ICommandPreProcessor<in TCommand>
    where TCommand : ICommandBase
{
    public Task ProcessAsync(TCommand command);
}
