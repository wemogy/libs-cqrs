using System;
using System.Threading.Tasks;
using Wemogy.CQRS.Commands.ValueObjects;

namespace Wemogy.CQRS.Commands.Abstractions;

public interface ICommands
{
    Task<TResult> RunAsync<TResult>(ICommand<TResult> command);

    Task RunAsync(ICommand command);

    // Task EnqueueAsync<TResult>(ICommand<TResult> command);

    /// <summary>
    /// Schedules a command to be executed after a specified delay in a background-job
    /// </summary>
    /// <returns>The ID of the scheduled job</returns>
    Task<string> ScheduleAsync<TCommand>(TCommand command, TimeSpan delay = default)
        where TCommand : ICommandBase;

    Task<string> ScheduleAsync<TCommand>(TCommand command, Func<TCommand, string> sessionIdResolver)
        where TCommand : ICommandBase;

    Task<string> ScheduleAsync<TCommand>(TCommand command, DelayOptions<TCommand> delayOptions)
        where TCommand : ICommandBase;

    Task<string> ScheduleAsync<TCommand>(TCommand command, ThrottleOptions<TCommand> throttleOptions)
        where TCommand : ICommandBase;

    Task DeleteScheduledAsync<TCommand>(string jobId)
        where TCommand : ICommandBase;

    /// <summary>
    /// Create a recurring background-job which executes the command every time the specified interval has passed.
    /// If there is already a recurring background-job with the same name, it will be replaced.
    /// </summary>
    /// <param name="id">The unique ID to identify the recurring job</param>
    /// <param name="command">The command which should be executed</param>
    /// <param name="cronExpression">The interval</param>
    Task ScheduleRecurringAsync(string id, ICommandBase command, string cronExpression);

    /// <summary>
    /// Removes a stored recurring background-job if it exists
    /// </summary>
    /// <param name="name">The unique name to identify the recurring job</param>
    Task DeleteRecurringAsync(string name);
}
