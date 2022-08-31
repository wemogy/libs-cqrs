using System.Threading.Tasks;
using FluentValidation;

namespace Wemogy.CQRS.Commands.Abstractions;

// ToDo: Rename to FluentValidationCommandValidator
public abstract class CommandValidatorBase<TCommand> : AbstractValidator<TCommand>, ICommandValidator<TCommand>
    where TCommand : ICommandBase
{
    public Task ValidateAsync(TCommand command)
    {
        return this.ValidateAndThrowAsync(command);
    }
}
