using System.Threading.Tasks;
using Wemogy.CQRS.Abstractions;
using Wemogy.CQRS.Commands.Abstractions;
using Wemogy.CQRS.Common.ValueObjects;

namespace Wemogy.CQRS.Commands.Runners;

public class VoidCommandRunner<TCommand>
    where TCommand : ICommand
{
    private readonly ICommandQueryDependencyResolver _commandQueryDependencyResolver;
    private readonly PreProcessingRunner<TCommand> _preProcessingRunner;
    private readonly ICommandHandler<TCommand> _commandHandler;
    private readonly VoidPostProcessingRunner<TCommand> _postProcessingRunner;
    private readonly IRemoteCommandRunner<TCommand>? _remoteCommandRunner;

    public VoidCommandRunner(
        ICommandQueryDependencyResolver commandQueryDependencyResolver,
        PreProcessingRunner<TCommand> preProcessingRunner,
        ICommandHandler<TCommand> commandHandler,
        VoidPostProcessingRunner<TCommand> postProcessingRunner,
        IRemoteCommandRunner<TCommand>? remoteCommandRunner = null)
    {
        _commandQueryDependencyResolver = commandQueryDependencyResolver;
        _preProcessingRunner = preProcessingRunner;
        _commandHandler = commandHandler;
        _postProcessingRunner = postProcessingRunner;
        _remoteCommandRunner = remoteCommandRunner;
    }

    public async Task RunAsync(TCommand command)
    {
        using var activity = Observability.DefaultActivities.StartActivity($"{typeof(TCommand).Name} Run");

        if (_remoteCommandRunner == null)
        {
            // pre-processing
            await _preProcessingRunner.RunAsync(command);

            // processing
            using var commandHandlerActivity = Observability.DefaultActivities.StartActivity($"{typeof(TCommand).Name} CommandHandler");
            await _commandHandler.HandleAsync(command);
            commandHandlerActivity?.Stop();

            // post-processing
            await _postProcessingRunner.RunAsync(command);
        }
        else
        {
            // build the CommandRequest
            var deps = _commandQueryDependencyResolver.ResolveDependencies();
            var commandRequest = new CommandRequest<TCommand>(
                command,
                deps);
            await _remoteCommandRunner.RunAsync(commandRequest);
        }
    }
}
