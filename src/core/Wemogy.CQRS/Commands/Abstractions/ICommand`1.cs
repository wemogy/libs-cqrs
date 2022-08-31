namespace Wemogy.CQRS.Commands.Abstractions;

public interface ICommand<out TResult> : ICommandBase
{
}
