using System.Threading.Tasks;
using Wemogy.Core.Errors;
using Wemogy.CQRS.Abstractions;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Commands.ValueObjects;
using Wemogy.CQRS.Common.ValueObjects;

namespace Wemogy.CQRS.Commands.Runners;

public class VoidScheduledCommandRunner<TCommand> : IScheduledCommandRunner<TCommand>
    where TCommand : ICommand
{
    private readonly ICommandQueryDependencyResolver _commandQueryDependencyResolver;
    private readonly PreProcessingRunner<TCommand> _preProcessingRunner;
    private readonly ICommandHandler<TCommand> _commandHandler;
    private readonly VoidPostProcessingRunner<TCommand> _postProcessingRunner;
    private readonly IScheduledCommandService? _scheduledCommandService;

    public VoidScheduledCommandRunner(
        ICommandQueryDependencyResolver commandQueryDependencyResolver,
        PreProcessingRunner<TCommand> preProcessingRunner,
        ICommandHandler<TCommand> commandHandler,
        VoidPostProcessingRunner<TCommand> postProcessingRunner,
        IScheduledCommandService? scheduledCommandService = null)
    {
        _commandQueryDependencyResolver = commandQueryDependencyResolver;
        _preProcessingRunner = preProcessingRunner;
        _commandHandler = commandHandler;
        _postProcessingRunner = postProcessingRunner;
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

        using var activity = Observability.DefaultActivities.StartActivity($"{typeof(TCommand).Name} Schedule");

        // pre-checking
        await _preProcessingRunner.RunPreChecksAsync(command);

        // build the scheduled command
        var deps = _commandQueryDependencyResolver.ResolveDependencies();
        var helper = new ScheduledCommand<TCommand>(
            deps,
            command);

        using var scheduledCommandServiceActivity = Observability.DefaultActivities.StartActivity($"{typeof(TCommand).Name} Scheduling");
        string jobId = await _scheduledCommandService.ScheduleAsync(
            this,
            helper,
            scheduleOptions);

        return jobId;
    }

    public async Task RunAsync(ScheduledCommand<TCommand> scheduledCommand)
    {
        using var activity = Observability.DefaultActivities.StartActivity($"{typeof(TCommand).Name} Run");
        var command = scheduledCommand.Command;

        // pre-processing
        await _preProcessingRunner.RunPreProcessorsAsync(command);

        // processing
        using var commandHandlerActivity = Observability.DefaultActivities.StartActivity($"{typeof(TCommand).Name} CommandHandler");
        await _commandHandler.HandleAsync(command);
        commandHandlerActivity?.Stop();

        // post-processing
        await _postProcessingRunner.RunAsync(command);
    }
}
