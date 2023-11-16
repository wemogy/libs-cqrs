using System.Threading.Tasks;
using Wemogy.CQRS.Common.ValueObjects;

namespace Wemogy.CQRS.Commands.Abstractions;

public interface IScheduledCommandRunner<TCommand>
    where TCommand : notnull
{
    Task RunAsync(ScheduledCommand<TCommand> scheduledCommand);
}
