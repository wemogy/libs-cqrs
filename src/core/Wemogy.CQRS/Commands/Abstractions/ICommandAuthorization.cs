using System.Threading.Tasks;

namespace Wemogy.CQRS.Commands.Abstractions;

public interface ICommandAuthorization<in TCommand>
    where TCommand : ICommandBase
{
    Task AuthorizeAsync(TCommand command);
}
