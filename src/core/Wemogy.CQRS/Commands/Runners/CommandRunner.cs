using System.Threading.Tasks;
using Wemogy.CQRS.Abstractions;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Common.ValueObjects;

namespace Wemogy.CQRS.Commands.Runners;

public class CommandRunner<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    private readonly ICommandQueryDependencyResolver _commandQueryDependencyResolver;
    private readonly PreProcessingRunner<TCommand> _preProcessingRunner;
    private readonly ICommandHandler<TCommand, TResult> _commandHandler;
    private readonly PostProcessingRunner<TCommand, TResult> _postProcessingRunner;
    private readonly IRemoteCommandRunner<TCommand, TResult>? _remoteCommandRunner;

    public CommandRunner(
        ICommandQueryDependencyResolver commandQueryDependencyResolver,
        PreProcessingRunner<TCommand> preProcessingRunner,
        ICommandHandler<TCommand, TResult> commandHandler,
        PostProcessingRunner<TCommand, TResult> postProcessingRunner,
        IRemoteCommandRunner<TCommand, TResult>? remoteCommandRunner = null)
    {
        _commandQueryDependencyResolver = commandQueryDependencyResolver;
        _preProcessingRunner = preProcessingRunner;
        _commandHandler = commandHandler;
        _postProcessingRunner = postProcessingRunner;
        _remoteCommandRunner = remoteCommandRunner;
    }

    public async Task<TResult> RunAsync(TCommand command)
    {
        using var activity = Observability.DefaultActivities.StartActivity($"{typeof(TCommand).Name} Run");
        TResult result;

        if (_remoteCommandRunner == null)
        {
            // pre-processing
            await _preProcessingRunner.RunAsync(command);

            // processing
            using var commandHandlerActivity = Observability.DefaultActivities.StartActivity($"{typeof(TCommand).Name} CommandHandler");
            result = await _commandHandler.HandleAsync(command);
            commandHandlerActivity?.Stop();

            // post-processing
            await _postProcessingRunner.RunAsync(command, result);
        }
        else
        {
            // build the CommandRequest
            var deps = _commandQueryDependencyResolver.ResolveDependencies();
            var commandRequest = new CommandRequest<TCommand>(
                command,
                deps);
            result = await _remoteCommandRunner.RunAsync(commandRequest);
        }

        return result;
    }
}
