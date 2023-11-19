using System.Threading.Tasks;
using Wemogy.Core.Errors;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Commands.ValueObjects;
using Wemogy.CQRS.Common.ValueObjects;

namespace Wemogy.CQRS.Commands.Runners;

public class ScheduledCommandRunner<TCommand, TResult> : IScheduledCommandRunner<TCommand>
    where TCommand : ICommand<TResult>
{
    private readonly IScheduledCommandDependencyResolver _scheduledCommandDependencyResolver;
    private readonly PreProcessingRunner<TCommand> _preProcessingRunner;
    private readonly ICommandHandler<TCommand, TResult> _commandHandler;
    private readonly PostProcessingRunner<TCommand, TResult> _postProcessingRunner;
    private readonly IScheduledCommandService? _scheduledCommandService;

    public ScheduledCommandRunner(
        IScheduledCommandDependencyResolver scheduledCommandDependencyResolver,
        PreProcessingRunner<TCommand> preProcessingRunner,
        ICommandHandler<TCommand, TResult> commandHandler,
        PostProcessingRunner<TCommand, TResult> postProcessingRunner,
        IScheduledCommandService? scheduledCommandService = null)
    {
        _scheduledCommandDependencyResolver = scheduledCommandDependencyResolver;
        _preProcessingRunner = preProcessingRunner;
        _commandHandler = commandHandler;
        _postProcessingRunner = postProcessingRunner;
        _scheduledCommandDependencyResolver = scheduledCommandDependencyResolver;
        _scheduledCommandService = scheduledCommandService;
    }

    public async Task<string> ScheduleAsync(TCommand command, ScheduleOptions<TCommand> scheduleOptions)
    {
        if (_scheduledCommandService == null)
        {
            throw Error.Unexpected(
                "ScheduledJobServiceNotRegistered",
                "ScheduledJobService is not registered. Please register it in the DI container.");
        }

        // pre-checking
        await _preProcessingRunner.RunPreChecksAsync(command);

        // build the scheduled command
        var deps = _scheduledCommandDependencyResolver.ResolveDependencies();
        var helper = new ScheduledCommand<TCommand>(
            deps,
            command);
        var jobId = await _scheduledCommandService.ScheduleAsync(
            this,
            helper,
            scheduleOptions);

        return jobId;
    }

    public async Task RunAsync(ScheduledCommand<TCommand> scheduledCommand)
    {
        var command = scheduledCommand.Command;

        // pre-processing
        await _preProcessingRunner.RunPreProcessorsAsync(command);

        // processing
        var result = await _commandHandler.HandleAsync(command);

        // post-processing
        await _postProcessingRunner.RunAsync(command, result);
    }
}
