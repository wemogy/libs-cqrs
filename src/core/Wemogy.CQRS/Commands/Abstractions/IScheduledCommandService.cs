using System.Threading.Tasks;
using Wemogy.CQRS.Commands.ValueObjects;
using Wemogy.CQRS.Common.ValueObjects;

namespace Wemogy.CQRS.Commands.Abstractions;

public interface IScheduledCommandService
{
    Task<string> ScheduleAsync<TCommand>(
        IScheduledCommandRunner<TCommand> scheduledCommandRunner,
        ScheduledCommand<TCommand> scheduledCommand,
        ScheduleOptions<TCommand> scheduleOptions)
        where TCommand : ICommandBase;

    Task DeleteAsync<TCommand>(string jobId)
        where TCommand : ICommandBase;
}
