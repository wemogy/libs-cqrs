using System.Threading.Tasks;

namespace Wemogy.CQRS.Commands.Abstractions;

public interface ICommands
{
    Task<TResult> RunAsync<TResult>(ICommand<TResult> command);

    Task<TResult> ScheduleDelayedAsync<TResult>(ICommand<TResult> command);

    /// <summary>
    /// Create a recurring command that will be executed every time the specified interval has passed.
    /// If there is already a recurring command with the same id, it will be replaced.
    /// </summary>
    /// <param name="recurringCommandId">The unique id to identify the recurring command</param>
    /// <param name="command">The command which should be executed</param>
    /// <param name="cronExpression">The interval</param>
    Task ScheduleRecurringAsync<TResult>(string recurringCommandId, ICommand<TResult> command, string cronExpression);

    /// <summary>
    /// Triggers a stored recurring command to be executed now.
    /// </summary>
    /// <param name="recurringCommandId">The unique id to identify the recurring command</param>
    Task TriggerRecurringAsync<TResult>(string recurringCommandId);

    /// <summary>
    /// Removes a stored recurring command
    /// </summary>
    /// <param name="recurringCommandId">The unique id to identify the recurring command</param>
    Task RemoveRecurringIfExistsAsync<TResult>(string recurringCommandId);
}
