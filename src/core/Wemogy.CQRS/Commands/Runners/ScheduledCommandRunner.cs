using System;
using System.Threading.Tasks;
using Wemogy.Core.Errors;
using Wemogy.CQRS.Commands.Abstractions;

namespace Wemogy.CQRS.Commands.Runners;

public class ScheduledCommandRunner<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    private readonly PreProcessingRunner<TCommand, TResult> _preProcessingRunner;
    private readonly ICommandHandler<TCommand, TResult> _commandHandler;
    private readonly PostProcessingRunner<TCommand, TResult> _postProcessingRunner;
    private readonly IScheduledCommandService? _scheduledCommandService;

    public ScheduledCommandRunner(
        PreProcessingRunner<TCommand, TResult> preProcessingRunner,
        ICommandHandler<TCommand, TResult> commandHandler,
        PostProcessingRunner<TCommand, TResult> postProcessingRunner,
        IScheduledCommandService? scheduledCommandService = null)
    {
        _preProcessingRunner = preProcessingRunner;
        _commandHandler = commandHandler;
        _postProcessingRunner = postProcessingRunner;
        _scheduledCommandService = scheduledCommandService;
    }

    public async Task<string> ScheduleAsync(TCommand command, TimeSpan delay)
    {
        if (_scheduledCommandService == null)
        {
            throw Error.Unexpected(
                "ScheduledJobServiceNotRegistered",
                "ScheduledJobService is not registered. Please register it in the DI container.");
        }

        // pre-checking
        await _preProcessingRunner.RunPreChecksAsync(command);

        // schedule scheduled command
        var jobId = await _scheduledCommandService.ScheduleAsync(
            () => RunAsync(command),
            delay);

        return jobId;
    }

    public async Task RunAsync(TCommand command)
    {
        // pre-processing
        await _preProcessingRunner.RunPreProcessorsAsync(command);

        // processing
        var result = await _commandHandler.HandleAsync(command);

        // post-processing
        await _postProcessingRunner.RunAsync(command, result);
    }
}
