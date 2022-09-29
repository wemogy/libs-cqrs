using System;
using System.Threading.Tasks;
using Wemogy.Core.Errors;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Commands.Registries;

namespace Wemogy.CQRS.Commands.Mediators;

public class CommandsMediator : ICommands
{
    private readonly CommandRunnerRegistry _commandRunnerRegistry;
    private readonly DelayedCommandRunnerRegistry _delayedCommandRunnerRegistry;
    private readonly RecurringCommandRunnerRegistry _recurringCommandRunnerRegistry;
    private readonly IDelayedJobService? _delayedJobService;
    private readonly IRecurringJobService? _recurringJobService;

    public CommandsMediator(
        CommandRunnerRegistry commandRunnerRegistry,
        DelayedCommandRunnerRegistry delayedCommandRunnerRegistry,
        RecurringCommandRunnerRegistry recurringCommandRunnerRegistry,
        IDelayedJobService? delayedJobService = null,
        IRecurringJobService? recurringJobService = null)
    {
        _commandRunnerRegistry = commandRunnerRegistry;
        _delayedCommandRunnerRegistry = delayedCommandRunnerRegistry;
        _recurringCommandRunnerRegistry = recurringCommandRunnerRegistry;
        _delayedJobService = delayedJobService;
        _recurringJobService = recurringJobService;
    }

    public Task<TResult> RunAsync<TResult>(ICommand<TResult> command)
    {
        return _commandRunnerRegistry.ExecuteCommandRunnerAsync(command);
    }

    public Task<string> ScheduleDelayedAsync<TResult>(ICommand<TResult> command, TimeSpan delay)
    {
        return _delayedCommandRunnerRegistry.ExecuteDelayedCommandRunnerAsync(command, delay);
    }

    public Task DeleteDelayedAsync(string jobId)
    {
        if (_delayedJobService == null)
        {
            throw Error.Unexpected(
                "DelayedJobServiceNotRegistered",
                "DelayedJobService is not registered. Please register it in the DI container.");
        }

        return _delayedJobService.CancelAsync(jobId);
    }

    public Task ScheduleRecurringAsync<TResult>(
        string recurringCommandId,
        ICommand<TResult> command,
        string cronExpression)
    {
        return _recurringCommandRunnerRegistry.ExecuteRecurringCommandRunnerAsync(
            recurringCommandId,
            command,
            cronExpression);
    }

    public Task TriggerRecurringAsync<TResult>(string recurringCommandId)
    {
        if (_recurringJobService == null)
        {
            throw Error.Unexpected(
                "RecurringJobServiceNotRegistered",
                "RecurringJobService is not registered. Please register it in the DI container.");
        }

        return _recurringJobService.TriggerAsync(recurringCommandId);
    }

    public Task DeleteRecurringIfExistsAsync<TResult>(string recurringCommandId)
    {
        if (_recurringJobService == null)
        {
            throw Error.Unexpected(
                "RecurringJobServiceNotRegistered",
                "RecurringJobService is not registered. Please register it in the DI container.");
        }

        return _recurringJobService.RemoveIfExistsAsync(recurringCommandId);
    }
}
