using System;
using System.Threading.Tasks;
using Wemogy.CQRS.Common.ValueObjects;

namespace Wemogy.CQRS.Commands.Abstractions;

public interface IScheduledCommandService
{
    Task<string> ScheduleAsync<TCommand>(
        IScheduledCommandRunner<TCommand> scheduledCommandRunner,
        ScheduledCommand<TCommand> scheduledCommand,
        TimeSpan delay)
        where TCommand : notnull;

    Task DeleteAsync<TCommand>(string jobId)
        where TCommand : ICommandBase;
}
