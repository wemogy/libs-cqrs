using System;
using System.Threading.Tasks;
using Wemogy.Core.Errors;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Commands.Registries;

namespace Wemogy.CQRS.Commands.Mediators;

public class CommandsMediator : ICommands
{
    private readonly CommandRunnerRegistry _commandRunnerRegistry;
    private readonly RecurringCommandRunnerRegistry _recurringCommandRunnerRegistry;
    private readonly IRecurringJobService? _recurringJobService;

    public CommandsMediator(
        CommandRunnerRegistry commandRunnerRegistry,
        RecurringCommandRunnerRegistry recurringCommandRunnerRegistry,
        IRecurringJobService? recurringJobService)
    {
        _commandRunnerRegistry = commandRunnerRegistry;
        _recurringCommandRunnerRegistry = recurringCommandRunnerRegistry;
        _recurringJobService = recurringJobService;
    }

    public Task<TResult> RunAsync<TResult>(ICommand<TResult> command)
    {
        return _commandRunnerRegistry.ExecuteCommandRunnerAsync(command);
    }

    public Task<TResult> ScheduleDelayedAsync<TResult>(ICommand<TResult> command)
    {
        throw new NotImplementedException();
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

    public Task RemoveRecurringIfExistsAsync<TResult>(string recurringCommandId)
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
