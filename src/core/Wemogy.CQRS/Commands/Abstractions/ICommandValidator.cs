using System.Threading.Tasks;

namespace Wemogy.CQRS.Commands.Abstractions;

public interface ICommandValidator<in TCommand>
    where TCommand : ICommandBase
{
    Task ValidateAsync(TCommand command);
}
