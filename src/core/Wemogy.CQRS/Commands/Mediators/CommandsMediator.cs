using System;
using System.Threading.Tasks;
using Wemogy.Core.Errors;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Commands.Registries;

namespace Wemogy.CQRS.Commands.Mediators;

public class CommandsMediator : ICommands
{
    private readonly CommandRunnerRegistry _commandRunnerRegistry;
    private readonly ScheduledCommandRunnerRegistry _scheduledCommandRunnerRegistry;
    private readonly RecurringCommandRunnerRegistry _recurringCommandRunnerRegistry;
    private readonly IScheduledCommandService? _scheduledCommandService;
    private readonly IRecurringCommandService? _recurringCommandService;

    public CommandsMediator(
        CommandRunnerRegistry commandRunnerRegistry,
        ScheduledCommandRunnerRegistry scheduledCommandRunnerRegistry,
        RecurringCommandRunnerRegistry recurringCommandRunnerRegistry,
        IScheduledCommandService? scheduledCommandService = null,
        IRecurringCommandService? recurringCommandService = null)
    {
        _commandRunnerRegistry = commandRunnerRegistry;
        _scheduledCommandRunnerRegistry = scheduledCommandRunnerRegistry;
        _recurringCommandRunnerRegistry = recurringCommandRunnerRegistry;
        _scheduledCommandService = scheduledCommandService;
        _recurringCommandService = recurringCommandService;
    }

    public Task<TResult> RunAsync<TResult>(ICommand<TResult> command)
    {
        return _commandRunnerRegistry.ExecuteCommandRunnerAsync(command);
    }

    public Task<string> ScheduleAsync<TResult>(ICommand<TResult> command, TimeSpan delay)
    {
        return _scheduledCommandRunnerRegistry.ExecuteScheduledCommandRunnerAsync(command, delay);
    }

    public Task DeleteScheduledAsync(string jobId)
    {
        if (_scheduledCommandService == null)
        {
            throw Error.Unexpected(
                "DelayedJobServiceNotRegistered",
                "DelayedJobService is not registered. Please register it in the DI container.");
        }

        return _scheduledCommandService.DeleteAsync(jobId);
    }

    public Task ScheduleRecurringAsync<TResult>(
        string name,
        ICommand<TResult> command,
        string cronExpression)
    {
        return _recurringCommandRunnerRegistry.ExecuteRecurringCommandRunnerAsync(
            name,
            command,
            cronExpression);
    }

    public Task DeleteRecurringAsync<TResult>(string name)
    {
        if (_recurringCommandService == null)
        {
            throw Error.Unexpected(
                "RecurringJobServiceNotRegistered",
                "RecurringJobService is not registered. Please register it in the DI container.");
        }

        return _recurringCommandService.RemoveIfExistsAsync(name);
    }
}
