using System;
using System.Threading.Tasks;
using Wemogy.Core.Errors;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Commands.Registries;
using Wemogy.CQRS.Commands.ValueObjects;

namespace Wemogy.CQRS.Commands.Mediators;

public class CommandsMediator : ICommands
{
    private readonly CommandRunnerRegistry _commandRunnerRegistry;
    private readonly ScheduledCommandRunnerRegistry _scheduledCommandRunnerRegistry;
    private readonly RecurringCommandRunnerRegistry _recurringCommandRunnerRegistry;
    private readonly IScheduledCommandService? _scheduledCommandService;
    private readonly IRecurringCommandService? _recurringCommandService;
    private readonly IServiceProvider _serviceProvider;

    public CommandsMediator(
        CommandRunnerRegistry commandRunnerRegistry,
        ScheduledCommandRunnerRegistry scheduledCommandRunnerRegistry,
        RecurringCommandRunnerRegistry recurringCommandRunnerRegistry,
        IServiceProvider serviceProvider,
        IScheduledCommandService? scheduledCommandService = null,
        IRecurringCommandService? recurringCommandService = null)
    {
        _commandRunnerRegistry = commandRunnerRegistry;
        _scheduledCommandRunnerRegistry = scheduledCommandRunnerRegistry;
        _recurringCommandRunnerRegistry = recurringCommandRunnerRegistry;
        _serviceProvider = serviceProvider;
        _scheduledCommandService = scheduledCommandService;
        _recurringCommandService = recurringCommandService;
    }

    public Task<TResult> RunAsync<TResult>(ICommand<TResult> command)
    {
        return _commandRunnerRegistry.ExecuteCommandRunnerAsync(_serviceProvider, command);
    }

    public Task RunAsync(ICommand command)
    {
        return _commandRunnerRegistry.ExecuteCommandRunnerAsync(_serviceProvider, command);
    }

    public Task<string> ScheduleAsync<TCommand>(TCommand command, TimeSpan delay = default)
        where TCommand : ICommandBase
    {
        return ScheduleAsync(command, new DelayOptions<TCommand>(delay));
    }

    public Task<string> ScheduleAsync<TCommand>(TCommand command, Func<TCommand, string> sessionIdResolver)
        where TCommand : ICommandBase
    {
        return ScheduleAsync(command, new DelayOptions<TCommand>(TimeSpan.Zero, sessionIdResolver));
    }

    public Task<string> ScheduleAsync<TCommand>(TCommand command, DelayOptions<TCommand> delayOptions)
        where TCommand : ICommandBase
    {
        return _scheduledCommandRunnerRegistry.ExecuteScheduledCommandRunnerAsync(
            _serviceProvider,
            command,
            new ScheduleOptions<TCommand>(delayOptions));
    }

    public Task<string> ScheduleAsync<TCommand>(TCommand command, ThrottleOptions<TCommand> throttleOptions)
        where TCommand : ICommandBase
    {
        return _scheduledCommandRunnerRegistry.ExecuteScheduledCommandRunnerAsync(
            _serviceProvider,
            command,
            new ScheduleOptions<TCommand>(throttleOptions));
    }

    public Task DeleteScheduledAsync<TCommand>(string jobId)
        where TCommand : ICommandBase
    {
        if (_scheduledCommandService == null)
        {
            throw Error.Unexpected(
                "DelayedJobServiceNotRegistered",
                "DelayedJobService is not registered. Please register it in the DI container.");
        }

        return _scheduledCommandService.DeleteAsync<TCommand>(jobId);
    }

    public Task ScheduleRecurringAsync(
        string name,
        ICommandBase command,
        string cronExpression)
    {
        return _recurringCommandRunnerRegistry.ExecuteRecurringCommandRunnerAsync(
            _serviceProvider,
            name,
            command,
            cronExpression);
    }

    public Task DeleteRecurringAsync(string name)
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
