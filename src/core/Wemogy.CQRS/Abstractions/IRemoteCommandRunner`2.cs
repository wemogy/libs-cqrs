using System.Threading.Tasks;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Common.ValueObjects;

namespace Wemogy.CQRS.Abstractions;

public interface IRemoteCommandRunner<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    Task<TResult> RunAsync(CommandRequest<TCommand> command);
}
