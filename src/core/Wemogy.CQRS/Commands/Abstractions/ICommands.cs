using System;
using System.Threading.Tasks;

namespace Wemogy.CQRS.Commands.Abstractions;

public interface ICommands
{
    Task<TResult> RunAsync<TResult>(ICommand<TResult> command);

    // Task EnqueueAsync<TResult>(ICommand<TResult> command);

    /// <summary>
    /// Schedules a command to be executed after a specified delay in a background-job
    /// </summary>
    /// <returns>The ID of the scheduled job</returns>
    Task<string> ScheduleAsync<TResult>(ICommand<TResult> command, TimeSpan delay);

    Task DeleteScheduledAsync(string jobId);

    /// <summary>
    /// Create a recurring background-job which executes the command every time the specified interval has passed.
    /// If there is already a recurring background-job with the same name, it will be replaced.
    /// </summary>
    /// <param name="name">The unique name to identify the recurring job</param>
    /// <param name="command">The command which should be executed</param>
    /// <param name="cronExpression">The interval</param>
    Task ScheduleRecurringAsync<TResult>(string name, ICommand<TResult> command, string cronExpression);

    /// <summary>
    /// Removes a stored recurring background-job if it exists
    /// </summary>
    /// <param name="name">The unique name to identify the recurring job</param>
    Task DeleteRecurringAsync<TResult>(string name);
}
