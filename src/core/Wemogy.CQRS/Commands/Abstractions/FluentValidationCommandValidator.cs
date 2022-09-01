using System.Threading.Tasks;
using FluentValidation;

namespace Wemogy.CQRS.Commands.Abstractions;

public abstract class FluentValidationCommandValidator<TCommand> : AbstractValidator<TCommand>, ICommandValidator<TCommand>
    where TCommand : ICommandBase
{
    public Task ValidateAsync(TCommand command)
    {
        return this.ValidateAndThrowAsync(command);
    }
}
