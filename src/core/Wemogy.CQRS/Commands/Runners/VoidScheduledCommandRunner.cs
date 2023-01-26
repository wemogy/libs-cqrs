using System;
using System.Threading.Tasks;
using Wemogy.Core.Errors;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Common.ValueObjects;

namespace Wemogy.CQRS.Commands.Runners;

public class VoidScheduledCommandRunner<TCommand>
    where TCommand : ICommand
{
    private readonly IScheduledCommandDependencyResolver _scheduledCommandDependencyResolver;
    private readonly PreProcessingRunner<TCommand> _preProcessingRunner;
    private readonly ICommandHandler<TCommand> _commandHandler;
    private readonly VoidPostProcessingRunner<TCommand> _postProcessingRunner;
    private readonly IScheduledCommandService? _scheduledCommandService;

    public VoidScheduledCommandRunner(
        IScheduledCommandDependencyResolver scheduledCommandDependencyResolver,
        PreProcessingRunner<TCommand> preProcessingRunner,
        ICommandHandler<TCommand> commandHandler,
        VoidPostProcessingRunner<TCommand> postProcessingRunner,
        IScheduledCommandService? scheduledCommandService = null)
    {
        _scheduledCommandDependencyResolver = scheduledCommandDependencyResolver;
        _preProcessingRunner = preProcessingRunner;
        _commandHandler = commandHandler;
        _postProcessingRunner = postProcessingRunner;
        _scheduledCommandDependencyResolver = scheduledCommandDependencyResolver;
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
        var deps = _scheduledCommandDependencyResolver.ResolveDependencies();
        var helper = new ScheduledCommand<TCommand>(
            deps,
            command);
        var jobId = await _scheduledCommandService.ScheduleAsync(
            () => RunAsync(helper),
            delay);

        return jobId;
    }

    public async Task RunAsync(ScheduledCommand<TCommand> scheduledCommand)
    {
        var command = scheduledCommand.Command;

        // pre-processing
        await _preProcessingRunner.RunPreProcessorsAsync(command);

        // processing
        await _commandHandler.HandleAsync(command);

        // post-processing
        await _postProcessingRunner.RunAsync(command);
    }
}
