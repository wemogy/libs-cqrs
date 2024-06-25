using System.Threading.Tasks;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Common.ValueObjects;

namespace Wemogy.CQRS.Abstractions;

public interface IRemoteCommandRunner<TCommand>
    where TCommand : ICommandBase
{
    Task RunAsync(CommandRequest<TCommand> request);
}
