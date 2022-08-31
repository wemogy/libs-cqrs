using Wemogy.CQRS.Commands.Structs;

namespace Wemogy.CQRS.Commands.Abstractions;

public interface ICommand : ICommand<Void>
{
}
